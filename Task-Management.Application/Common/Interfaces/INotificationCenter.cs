using Task_Management.Domain.Enums;

namespace Task_Management.Application.Common.Interfaces;

// One notification for one user. Payload is the object the apps get (it's
// stored as JSON). EmailCategory set = this one is worth an email to this
// user, subject to their email settings.
public record NewNotification(int UserId, NotificationType Type, object Payload, EmailCategory? EmailCategory = null);

// Implemented in Infrastructure: saves notifications to each user's list,
// pushes them live to the bell, and emails them now or in the daily digest
// according to each user's email settings. Never throws.
public interface INotificationCenter
{
    Task PublishAsync(IReadOnlyCollection<NewNotification> notifications);
}
