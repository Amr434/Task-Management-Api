using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Notifications.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Infrastructure.Data;
using Task_Management.Infrastructure.Email;

namespace Task_Management.Infrastructure.Notifications;

// Time-based notifications, checked every few minutes and sent once the
// day's send hour has passed (team time zone, see NotificationScheduleSettings):
//  - "due tomorrow" and "overdue" reminders to a task's assignees;
//  - the daily digest for users who chose it instead of instant emails;
//  - the daily or weekly "your tasks" summary.
// Each step records what it sent (on the task or the user), so a check that
// runs again, or after a restart, never sends twice.
public class ScheduledNotificationJobs : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    // Overdue reminders only for tasks that became overdue in the last week,
    // so turning this on doesn't email about every old unfinished task.
    private const int OverdueLookbackDays = 7;
    private const int SummaryDays = 7;
    private const int MaxDigestLines = 50;

    private readonly IServiceScopeFactory _scopes;
    private readonly NotificationScheduleSettings _schedule;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<ScheduledNotificationJobs> _logger;

    public ScheduledNotificationJobs(IServiceScopeFactory scopes, IOptions<NotificationScheduleSettings> schedule,
        IOptions<EmailSettings> emailSettings, ILogger<ScheduledNotificationJobs> logger)
    {
        _scopes = scopes;
        _schedule = schedule.Value;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let startup (migrations, seeding) finish first.
        try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
        catch (OperationCanceledException) { return; }

        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled notifications failed; retrying next round.");
            }
        }
        while (await WaitAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try { return await timer.WaitForNextTickAsync(ct); }
        catch (OperationCanceledException) { return false; }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        var tz = _schedule.ResolveTimeZone();
        var nowUtc = DateTime.UtcNow;
        var today = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz).Date;
        var sendTimeTodayUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(today.AddHours(Math.Clamp(_schedule.SendHour, 0, 23)), DateTimeKind.Unspecified), tz);

        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();
        var center = scope.ServiceProvider.GetRequiredService<INotificationCenter>();
        var emails = scope.ServiceProvider.GetRequiredService<IEmailQueue>();

        await DeleteOldNotificationsAsync(db, nowUtc, ct);

        if (nowUtc < sendTimeTodayUtc) return;

        await SendDueRemindersAsync(db, center, today, ct);
        await SendDigestsAsync(db, emails, nowUtc, sendTimeTodayUtc, ct);
        await SendSummariesAsync(db, emails, nowUtc, today, sendTimeTodayUtc, tz, ct);
    }

    private async Task DeleteOldNotificationsAsync(TaskManagementDbContext db, DateTime nowUtc, CancellationToken ct)
    {
        var cutoff = nowUtc.AddDays(-Math.Max(1, _schedule.KeepReadNotificationsDays));
        await db.Notifications
            .Where(n => n.ReadAtUtc != null && n.CreatedAtUtc < cutoff)
            .ExecuteDeleteAsync(ct);
    }

    // ---- Due date reminders ----

    private async Task SendDueRemindersAsync(TaskManagementDbContext db, INotificationCenter center, DateTime today, CancellationToken ct)
    {
        var tomorrow = today.AddDays(1);
        var dayAfter = today.AddDays(2);
        var overdueFrom = today.AddDays(-OverdueLookbackDays);

        // Due dates are calendar dates (stored at midnight, or around noon
        // from the web app), so a day is the range [day, day + 1).
        var tasks = await db.TaskItems
            .Include(t => t.Assignees)
            .Where(t => t.Status != TaskStatusLevel.Complete && t.DueDate != null
                && ((t.DueDate >= tomorrow && t.DueDate < dayAfter
                        && (t.DueSoonReminderSentFor == null || t.DueSoonReminderSentFor != t.DueDate))
                    || (t.DueDate >= overdueFrom && t.DueDate < today
                        && (t.OverdueReminderSentFor == null || t.OverdueReminderSentFor != t.DueDate))))
            .ToListAsync(ct);
        if (tasks.Count == 0) return;

        var notifications = new List<NewNotification>();
        foreach (var task in tasks)
        {
            var overdue = task.DueDate < today;
            if (overdue) task.OverdueReminderSentFor = task.DueDate;
            else task.DueSoonReminderSentFor = task.DueDate;

            var payload = new TaskDueDto
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                ProjectId = task.ProjectId,
                DueDate = task.DueDate!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            };
            var type = overdue ? NotificationType.TaskOverdue : NotificationType.TaskDueSoon;
            notifications.AddRange(task.Assignees
                .Where(u => u.IsActive)
                .Select(u => new NewNotification(u.Id, type, payload, EmailCategory.DueReminders)));
        }

        // Recorded before sending: if sending fails, no one gets the reminder
        // twice (better than a stuck task emailing every 10 minutes).
        await db.SaveChangesAsync(ct);
        await center.PublishAsync(notifications);
        _logger.LogInformation("Sent due date reminders for {Count} task(s).", tasks.Count);
    }

    // ---- Daily digest ----

    private async Task SendDigestsAsync(TaskManagementDbContext db, IEmailQueue emails, DateTime nowUtc,
        DateTime sendTimeTodayUtc, CancellationToken ct)
    {
        var users = await db.Users
            .Where(u => u.IsActive && u.EmailMode == EmailDeliveryMode.DailyDigest
                && (u.LastDigestSentAtUtc == null || u.LastDigestSentAtUtc < sendTimeTodayUtc))
            .ToListAsync(ct);

        foreach (var user in users)
        {
            var since = user.LastDigestSentAtUtc ?? nowUtc.AddDays(-1);
            var rows = await db.Notifications.AsNoTracking()
                .Where(n => n.UserId == user.Id && n.EmailCategory != null && n.CreatedAtUtc > since && n.CreatedAtUtc <= nowUtc)
                .OrderBy(n => n.Id)
                .ToListAsync(ct);

            var lines = rows
                .Where(n => user.WantsEmail(n.EmailCategory!.Value))
                .Select(n => NotificationEmailText.Build(n.Type, n.Payload))
                .Where(e => e is not null)
                .Select(e => new EmailListItem(e!.Message, Link(e.LinkPath)))
                .ToList();

            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(user.Email))
            {
                var shown = lines.Take(MaxDigestLines).ToList();
                var intro = lines.Count == 1
                    ? "Here's what happened in Task Management since your last digest."
                    : $"Here are the {lines.Count} things that happened in Task Management since your last digest.";
                if (lines.Count > shown.Count) intro += $" The latest {shown.Count} are below.";

                var subject = lines.Count == 1 ? "Your daily digest: 1 update" : $"Your daily digest: {lines.Count} updates";
                var html = EmailTemplates.List(subject, intro,
                    new[] { new EmailListSection("Updates", shown) }, "Open Task Management", Link("/"));
                emails.Enqueue(new EmailMessage(user.Email, subject, html));
            }

            user.LastDigestSentAtUtc = nowUtc;
        }

        if (users.Count > 0) await db.SaveChangesAsync(ct);
    }

    // ---- Daily / weekly summary ----

    private async Task SendSummariesAsync(TaskManagementDbContext db, IEmailQueue emails, DateTime nowUtc, DateTime today,
        DateTime sendTimeTodayUtc, TimeZoneInfo tz, CancellationToken ct)
    {
        // The weekly summary's period starts at the send time on the last
        // summary day (today, if today is that day).
        var daysSinceSummaryDay = ((int)today.DayOfWeek - (int)_schedule.WeeklySummaryDay + 7) % 7;
        var weekStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(today.AddDays(-daysSinceSummaryDay).AddHours(Math.Clamp(_schedule.SendHour, 0, 23)), DateTimeKind.Unspecified), tz);

        var users = await db.Users
            .Where(u => u.IsActive
                && ((u.EmailSummary == SummaryFrequency.Daily
                        && (u.LastSummarySentAtUtc == null || u.LastSummarySentAtUtc < sendTimeTodayUtc))
                    || (u.EmailSummary == SummaryFrequency.Weekly
                        && (u.LastSummarySentAtUtc == null || u.LastSummarySentAtUtc < weekStartUtc))))
            .ToListAsync(ct);
        if (users.Count == 0) return;

        var until = today.AddDays(SummaryDays);
        foreach (var user in users)
        {
            var tasks = await db.TaskItems.AsNoTracking()
                .Where(t => t.Status != TaskStatusLevel.Complete && t.DueDate != null && t.DueDate < until
                    && t.Assignees.Any(a => a.Id == user.Id))
                .OrderBy(t => t.DueDate)
                .Select(t => new { t.Id, t.Title, t.ProjectId, DueDate = t.DueDate!.Value })
                .ToListAsync(ct);

            if (tasks.Count > 0 && !string.IsNullOrWhiteSpace(user.Email))
            {
                EmailListItem Line(string text, int projectId) => new(text, Link(NotificationEmailText.ProjectLink(projectId)));
                string DateOf(DateTime d) => d.ToString("ddd d MMM", CultureInfo.InvariantCulture);

                var overdue = tasks.Where(t => t.DueDate < today)
                    .Select(t => Line($"{t.Title} (was due {DateOf(t.DueDate)})", t.ProjectId)).ToList();
                var dueToday = tasks.Where(t => t.DueDate >= today && t.DueDate < today.AddDays(1))
                    .Select(t => Line(t.Title, t.ProjectId)).ToList();
                var upcoming = tasks.Where(t => t.DueDate >= today.AddDays(1))
                    .Select(t => Line($"{t.Title} ({DateOf(t.DueDate)})", t.ProjectId)).ToList();

                var dueThisWeek = dueToday.Count + upcoming.Count;
                var subject = dueThisWeek == 0
                    ? $"You have {Plural(overdue.Count, "overdue task")}"
                    : $"You have {Plural(dueThisWeek, "task")} due this week"
                        + (overdue.Count > 0 ? $", {overdue.Count} overdue" : string.Empty);
                var greeting = string.IsNullOrWhiteSpace(user.FirstName) ? "Hi," : $"Hi {user.FirstName},";
                var html = EmailTemplates.List(subject,
                    $"{greeting} here are your unfinished tasks that are due in the next {SummaryDays} days or are already late.",
                    new[]
                    {
                        new EmailListSection("Overdue", overdue),
                        new EmailListSection("Due today", dueToday),
                        new EmailListSection("Coming up", upcoming),
                    },
                    "Open my tasks", Link("/my-tasks/assigned"));
                emails.Enqueue(new EmailMessage(user.Email, subject, html));
            }

            user.LastSummarySentAtUtc = nowUtc;
        }

        await db.SaveChangesAsync(ct);
    }

    private string Link(string path) => _emailSettings.AppBaseUrl.TrimEnd('/') + path;

    private static string Plural(int count, string word) => count == 1 ? $"1 {word}" : $"{count} {word}s";
}
