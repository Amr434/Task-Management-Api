using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Notifications;

// A page of the user's notification list, newest first. beforeId continues
// from the last item of the previous page.
public class NotificationsForUserSpecification : BaseSpecification<Notification>
{
    public NotificationsForUserSpecification(int userId, int? beforeId, int take)
        : base(n => n.UserId == userId && (beforeId == null || n.Id < beforeId))
    {
        AddOrderByDescending(n => n.Id);
        ApplyPaging(0, take);
    }
}

// The user's unread notifications (all, or just one), for the badge and for
// marking them read.
public class UnreadNotificationsForUserSpecification : BaseSpecification<Notification>
{
    public UnreadNotificationsForUserSpecification(int userId, int? notificationId = null)
        : base(n => n.UserId == userId && n.ReadAtUtc == null
            && (notificationId == null || n.Id == notificationId))
    {
    }
}
