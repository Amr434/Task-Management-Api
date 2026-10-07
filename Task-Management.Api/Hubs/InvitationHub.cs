using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Task_Management.Api.Hubs;

[Authorize]
public class InvitationHub : Hub
{
    // The Hub uses the user's ID (from the JWT claims) to map connections to users.
    // By default, SignalR uses ClaimTypes.NameIdentifier as the UserIdentifier.
    // Clients just need to connect and they will receive events sent to their User ID.
}
