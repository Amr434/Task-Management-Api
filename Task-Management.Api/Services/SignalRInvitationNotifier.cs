using Microsoft.AspNetCore.SignalR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Api.Hubs;

namespace Task_Management.Api.Services;

public class SignalRInvitationNotifier : IInvitationNotifier
{
    private readonly IHubContext<InvitationHub> _hubContext;

    public SignalRInvitationNotifier(IHubContext<InvitationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task InvitationReceivedAsync(int inviteeUserId, InvitationDto invitation)
    {
        await _hubContext.Clients.User(inviteeUserId.ToString())
            .SendAsync("ReceiveInvitation", invitation);
    }

    public async Task InvitationRespondedAsync(int inviterUserId, InvitationDto invitation)
    {
        await _hubContext.Clients.User(inviterUserId.ToString())
            .SendAsync("InvitationResponded", invitation);
    }
}
