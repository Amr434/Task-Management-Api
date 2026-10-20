using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Infrastructure.Notifications;

// These turn the same events as the live pop-ups into saved notifications
// (see NotificationCenter), and decide which recipients each is worth an
// email to. The Api layer runs them next to the SignalR notifiers.

public class InboxTaskNotifier : ITaskNotifier
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationCenter _center;

    public InboxTaskNotifier(IUnitOfWork unitOfWork, INotificationCenter center)
    {
        _unitOfWork = unitOfWork;
        _center = center;
    }

    public async Task TaskChangedAsync(IEnumerable<int> recipientUserIds, TaskChangeDto change)
    {
        var a = change.Activity;
        // History stores the person an assignment is about by name: the removed
        // person as the old value, the added one as the new value.
        var (category, affectedPerson) = a.Type switch
        {
            TaskActivityType.AssigneeAdded => (EmailCategory.Assignments, a.NewValue),
            TaskActivityType.AssigneeRemoved => (EmailCategory.Assignments, a.OldValue),
            TaskActivityType.CommentAssigned => (EmailCategory.Comments, a.NewValue),
            TaskActivityType.StatusChanged or TaskActivityType.DueDateChanged => (EmailCategory.TaskUpdates, (string?)null),
            _ => ((EmailCategory?)null, (string?)null),
        };

        var notifications = new List<NewNotification>();
        foreach (var userId in recipientUserIds.Distinct())
        {
            var emailCategory = category;
            if (category is not null && affectedPerson is not null)
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                if (user is null || PersonName(user) != affectedPerson) emailCategory = null;
            }
            notifications.Add(new NewNotification(userId, NotificationType.TaskChanged, change, emailCategory));
        }

        await _center.PublishAsync(notifications);
    }

    // Same format TaskHistory uses for names.
    private static string PersonName(User user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? user.Email : name;
    }
}

public class InboxCommentNotifier : ICommentNotifier
{
    private readonly INotificationCenter _center;

    public InboxCommentNotifier(INotificationCenter center)
    {
        _center = center;
    }

    // Mentioned people get a "mentioned you" entry instead of "commented".
    // Only the person the comment is assigned to gets a "commented" email.
    public Task CommentAddedAsync(IEnumerable<int> recipientUserIds, CommentDto comment, IReadOnlyCollection<int> mentionedUserIds)
    {
        var notifications = recipientUserIds.Distinct().Select(userId => mentionedUserIds.Contains(userId)
            ? new NewNotification(userId, NotificationType.Mentioned, comment, EmailCategory.Mentions)
            : new NewNotification(userId, NotificationType.CommentAdded, comment,
                comment.AssignedTo?.Id == userId ? EmailCategory.Comments : null))
            .ToList();

        return _center.PublishAsync(notifications);
    }
}

public class InboxInvitationNotifier : IInvitationNotifier
{
    private readonly INotificationCenter _center;

    public InboxInvitationNotifier(INotificationCenter center)
    {
        _center = center;
    }

    public Task InvitationReceivedAsync(int inviteeUserId, InvitationDto invitation) =>
        _center.PublishAsync(new[]
        {
            new NewNotification(inviteeUserId, NotificationType.InvitationReceived, invitation, EmailCategory.Invitations),
        });

    public Task InvitationRespondedAsync(int inviterUserId, InvitationDto invitation) =>
        _center.PublishAsync(new[]
        {
            new NewNotification(inviterUserId, NotificationType.InvitationResponded, invitation, EmailCategory.Invitations),
        });
}
