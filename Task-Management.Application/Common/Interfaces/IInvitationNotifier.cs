using Task_Management.Application.Features.Invitations.DTOs;

namespace Task_Management.Application.Common.Interfaces;

// Implemented in the Api layer with SignalR: pushes real-time events to a
// specific user's open sessions. No-ops if the user isn't connected.
public interface IInvitationNotifier
{
    Task InvitationReceivedAsync(int inviteeUserId, InvitationDto invitation);
    Task InvitationRespondedAsync(int inviterUserId, InvitationDto invitation);
}
