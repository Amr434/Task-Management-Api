using Microsoft.AspNetCore.SignalR;
using Task_Management.Api.Hubs;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Notifications.DTOs;

namespace Task_Management.Api.Services;

// Uses the same hub connection the apps already open for invitations.
public class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<InvitationHub> _hubContext;

    public SignalRNotificationPusher(IHubContext<InvitationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotificationCreatedAsync(int userId, NotificationDto notification) =>
        _hubContext.Clients.User(userId.ToString()).SendAsync("NotificationCreated", notification);
}
