using Task_Management.Application.Features.Notifications.DTOs;

namespace Task_Management.Application.Common.Interfaces;

// Implemented in the Api layer with SignalR: adds a saved notification to the
// user's open bell right away. No-ops for users who aren't connected.
public interface INotificationPusher
{
    Task NotificationCreatedAsync(int userId, NotificationDto notification);
}
