using Microsoft.Extensions.Options;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Infrastructure.Email;

// Emails only the task changes worth an inbox message. Assignment changes go
// just to the person assigned/unassigned; status and due date changes go to
// everyone the live notification goes to (assignees, never the actor).
public class EmailTaskNotifier : ITaskNotifier
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailQueue _queue;
    private readonly EmailSettings _settings;

    public EmailTaskNotifier(IUnitOfWork unitOfWork, IEmailQueue queue, IOptions<EmailSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _queue = queue;
        _settings = settings.Value;
    }

    public async Task TaskChangedAsync(IEnumerable<int> recipientUserIds, TaskChangeDto change)
    {
        var activity = change.Activity;
        var type = activity.Type;
        if (type is not (TaskActivityType.AssigneeAdded or TaskActivityType.AssigneeRemoved
            or TaskActivityType.StatusChanged or TaskActivityType.DueDateChanged
            or TaskActivityType.CommentAssigned))
        {
            return;
        }

        var recipients = await EmailRecipients.LoadAsync(_unitOfWork, recipientUserIds);

        // History stores the affected person by name (the removed person as the
        // old value, the added one as the new value), so match on that.
        var affectedPerson = type switch
        {
            TaskActivityType.AssigneeRemoved => activity.OldValue,
            TaskActivityType.AssigneeAdded or TaskActivityType.CommentAssigned => activity.NewValue,
            _ => null,
        };
        if (affectedPerson is not null)
        {
            recipients = recipients.Where(u => EmailRecipients.Name(u) == affectedPerson).ToList();
        }
        if (recipients.Count == 0) return;

        var actor = activity.User is null
            ? "Someone"
            : $"{activity.User.FirstName} {activity.User.LastName}".Trim();
        var title = change.TaskTitle;

        var (subject, message) = type switch
        {
            TaskActivityType.AssigneeAdded =>
                ($"You were assigned to \"{title}\"", $"{actor} assigned you to the task \"{title}\"."),
            TaskActivityType.AssigneeRemoved =>
                ($"You were removed from \"{title}\"", $"{actor} removed you from the task \"{title}\"."),
            TaskActivityType.StatusChanged =>
                ($"\"{title}\" is now {StatusText(activity.NewValue)}",
                 $"{actor} changed the status of \"{title}\" from {StatusText(activity.OldValue)} to {StatusText(activity.NewValue)}."),
            TaskActivityType.DueDateChanged =>
                ($"Due date changed for \"{title}\"",
                 $"{actor} changed the due date of \"{title}\" from {DateText(activity.OldValue)} to {DateText(activity.NewValue)}."),
            _ =>
                ($"A comment on \"{title}\" was assigned to you", $"{actor} assigned you a comment on the task \"{title}\"."),
        };

        var link = $"{_settings.AppBaseUrl.TrimEnd('/')}/projects/{change.ProjectId}";
        var html = EmailTemplates.Notification(subject, message, null, "Open task", link);

        foreach (var user in recipients)
        {
            _queue.Enqueue(new EmailMessage(user.Email, subject, html));
        }
    }

    // History stores enum names ("InProgress").
    private static string StatusText(string? value) => value switch
    {
        nameof(TaskStatusLevel.ToDo) => "To Do",
        nameof(TaskStatusLevel.InProgress) => "In Progress",
        nameof(TaskStatusLevel.Complete) => "Complete",
        null or "" => "none",
        _ => value,
    };

    private static string DateText(string? value) => string.IsNullOrEmpty(value) ? "none" : value;
}
