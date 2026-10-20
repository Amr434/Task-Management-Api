using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Notifications;
using Task_Management.Application.Features.Notifications.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Infrastructure.Data;
using Task_Management.Infrastructure.Email;

namespace Task_Management.Infrastructure.Notifications;

// Every notification goes through here: it's saved to the user's list (the
// bell), pushed live to their open apps, and emailed now if they get instant
// emails of that kind. Digest users get it in the daily digest instead (see
// ScheduledNotificationJobs), which reads the saved rows.
public class NotificationCenter : INotificationCenter
{
    private readonly IServiceScopeFactory _scopes;
    private readonly INotificationPusher _pusher;
    private readonly IEmailQueue _emails;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<NotificationCenter> _logger;

    public NotificationCenter(IServiceScopeFactory scopes, INotificationPusher pusher, IEmailQueue emails,
        IOptions<EmailSettings> emailSettings, ILogger<NotificationCenter> logger)
    {
        _scopes = scopes;
        _pusher = pusher;
        _emails = emails;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task PublishAsync(IReadOnlyCollection<NewNotification> notifications)
    {
        if (notifications.Count == 0) return;
        try
        {
            // A context of our own: the caller's may hold changes it hasn't
            // saved, which must not be saved along with these.
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

            var now = DateTime.UtcNow;
            var rows = notifications.Select(n => new Notification
            {
                UserId = n.UserId,
                Type = n.Type,
                Payload = JsonSerializer.Serialize(n.Payload, n.Payload.GetType(), NotificationDto.PayloadJson),
                EmailCategory = n.EmailCategory,
                CreatedAtUtc = now,
            }).ToList();

            db.Notifications.AddRange(rows);
            await db.SaveChangesAsync();

            foreach (var row in rows)
            {
                try
                {
                    await _pusher.NotificationCreatedAsync(row.UserId, NotificationMapping.ToDto(row));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to push notification {Id} live.", row.Id);
                }
            }

            await EmailNowAsync(db, rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {Count} notification(s).", notifications.Count);
        }
    }

    private async Task EmailNowAsync(TaskManagementDbContext db, List<Notification> rows)
    {
        var emailable = rows.Where(r => r.EmailCategory != null).ToList();
        if (emailable.Count == 0) return;

        var userIds = emailable.Select(r => r.UserId).Distinct().ToList();
        var users = await db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        foreach (var row in emailable)
        {
            if (!users.TryGetValue(row.UserId, out var user) || !ShouldEmailNow(user, row.EmailCategory!.Value)) continue;

            var email = NotificationEmailText.Build(row.Type, row.Payload);
            if (email is null) continue;

            var html = EmailTemplates.Notification(email.Subject, email.Message, email.Detail, email.ButtonText,
                _emailSettings.AppBaseUrl.TrimEnd('/') + email.LinkPath);
            _emails.Enqueue(new EmailMessage(user.Email, email.Subject, html));
        }
    }

    private static bool ShouldEmailNow(User user, EmailCategory category) =>
        user.IsActive
        && !string.IsNullOrWhiteSpace(user.Email)
        && user.EmailMode == EmailDeliveryMode.Instant
        && user.WantsEmail(category);
}
