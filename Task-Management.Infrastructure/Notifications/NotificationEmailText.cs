using System.Globalization;
using System.Text.Json;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Application.Features.Notifications.DTOs;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Enums;

namespace Task_Management.Infrastructure.Notifications;

// What an email about one notification says. LinkPath is relative to the web
// app (EmailSettings.AppBaseUrl).
internal record NotificationEmail(string Subject, string Message, string? Detail, string ButtonText, string LinkPath);

// Email wording for a saved notification (type + JSON payload), used for the
// instant emails and for the lines of the daily digest.
internal static class NotificationEmailText
{
    public static NotificationEmail? Build(NotificationType type, string payloadJson)
    {
        try
        {
            return type switch
            {
                NotificationType.TaskChanged => TaskChanged(Read<TaskChangeDto>(payloadJson)),
                NotificationType.CommentAdded => CommentAssigned(Read<CommentDto>(payloadJson)),
                NotificationType.Mentioned => Mentioned(Read<CommentDto>(payloadJson)),
                NotificationType.InvitationReceived => InvitationReceived(Read<InvitationDto>(payloadJson)),
                NotificationType.InvitationResponded => InvitationResponded(Read<InvitationDto>(payloadJson)),
                NotificationType.TaskDueSoon => DueSoon(Read<TaskDueDto>(payloadJson)),
                NotificationType.TaskOverdue => Overdue(Read<TaskDueDto>(payloadJson)),
                _ => null,
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static T Read<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, NotificationDto.PayloadJson) ?? throw new JsonException("Empty payload.");

    private static NotificationEmail? TaskChanged(TaskChangeDto change)
    {
        var a = change.Activity;
        var actor = Name(a.User);
        var title = change.TaskTitle;
        var link = ProjectLink(change.ProjectId);

        return a.Type switch
        {
            TaskActivityType.AssigneeAdded => new($"You were assigned to \"{title}\"",
                $"{actor} assigned you to the task \"{title}\".", null, "Open task", link),
            TaskActivityType.AssigneeRemoved => new($"You were removed from \"{title}\"",
                $"{actor} removed you from the task \"{title}\".", null, "Open task", link),
            TaskActivityType.StatusChanged => new($"\"{title}\" is now {StatusText(a.NewValue)}",
                $"{actor} changed the status of \"{title}\" from {StatusText(a.OldValue)} to {StatusText(a.NewValue)}.",
                null, "Open task", link),
            TaskActivityType.DueDateChanged => new($"Due date changed for \"{title}\"",
                $"{actor} changed the due date of \"{title}\" from {DateText(a.OldValue)} to {DateText(a.NewValue)}.",
                null, "Open task", link),
            TaskActivityType.CommentAssigned => new($"A comment on \"{title}\" was assigned to you",
                $"{actor} assigned you a comment on the task \"{title}\".", null, "View comment", "/assigned-comments"),
            _ => new($"\"{title}\" was updated", $"{actor} updated the task \"{title}\".", null, "Open task", link),
        };
    }

    // Comment notifications are only emailed to the person the comment is assigned to.
    private static NotificationEmail CommentAssigned(CommentDto comment)
    {
        var author = Name(comment.Author);
        var task = TaskName(comment.TaskTitle);
        return new($"{author} assigned you a comment on {task}", $"{author} wrote on {task}:",
            comment.Text, "View comment", "/assigned-comments");
    }

    private static NotificationEmail Mentioned(CommentDto comment)
    {
        var author = Name(comment.Author);
        var task = TaskName(comment.TaskTitle);
        return new($"{author} mentioned you on {task}", $"{author} mentioned you in a comment on {task}:",
            comment.Text, "View comment", ProjectLink(comment.ProjectId));
    }

    private static NotificationEmail InvitationReceived(InvitationDto invitation)
    {
        var inviter = string.IsNullOrWhiteSpace(invitation.InviterName) ? "Someone" : invitation.InviterName;
        var target = TargetLabel(invitation);
        return new($"{inviter} invited you to {target}",
            $"{inviter} invited you to join {target}. Open the app to accept or decline.", null, "View invitation", "/");
    }

    private static NotificationEmail InvitationResponded(InvitationDto invitation)
    {
        var invitee = string.IsNullOrWhiteSpace(invitation.InviteeName) ? "Someone" : invitation.InviteeName;
        var verb = invitation.Status == (int)InvitationStatus.Accepted ? "accepted" : "declined";
        return new($"{invitee} {verb} your invitation",
            $"{invitee} {verb} your invitation to {TargetLabel(invitation)}.", null, "Open Task Management", "/");
    }

    private static NotificationEmail DueSoon(TaskDueDto due) =>
        new($"\"{due.TaskTitle}\" is due tomorrow",
            $"The task \"{due.TaskTitle}\" is due tomorrow ({DateText(due.DueDate)}).", null, "Open task", ProjectLink(due.ProjectId));

    private static NotificationEmail Overdue(TaskDueDto due) =>
        new($"\"{due.TaskTitle}\" is overdue",
            $"The task \"{due.TaskTitle}\" was due on {DateText(due.DueDate)} and isn't complete yet.", null, "Open task", ProjectLink(due.ProjectId));

    public static string ProjectLink(int projectId) => $"/projects/{projectId}";

    private static string Name(UserDto? user)
    {
        if (user is null) return "Someone";
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? user.Email : name;
    }

    private static string TaskName(string? title) => string.IsNullOrWhiteSpace(title) ? "a task" : $"\"{title}\"";

    private static string TargetLabel(InvitationDto invitation)
    {
        var kind = invitation.TargetType == (int)InvitationTargetType.Space ? "space" : "project";
        return string.IsNullOrWhiteSpace(invitation.TargetName) ? $"a {kind}" : $"the {kind} \"{invitation.TargetName}\"";
    }

    // History stores status as enum names ("InProgress").
    private static string StatusText(string? value) => value switch
    {
        nameof(TaskStatusLevel.ToDo) => "To Do",
        nameof(TaskStatusLevel.InProgress) => "In Progress",
        nameof(TaskStatusLevel.Complete) => "Complete",
        null or "" => "none",
        _ => value,
    };

    // yyyy-MM-dd → "8 Oct 2026".
    public static string DateText(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "none";
        return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("d MMM yyyy", CultureInfo.InvariantCulture)
            : value;
    }
}
