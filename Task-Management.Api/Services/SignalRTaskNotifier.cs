using Microsoft.AspNetCore.SignalR;
using Task_Management.Api.Hubs;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Tasks.DTOs;

namespace Task_Management.Api.Services;

// Uses the same hub connection the apps already open for invitations.
public class SignalRTaskNotifier : ITaskNotifier
{
    private readonly IHubContext<InvitationHub> _hubContext;

    public SignalRTaskNotifier(IHubContext<InvitationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task TaskChangedAsync(IEnumerable<int> recipientUserIds, TaskChangeDto change)
    {
        var ids = recipientUserIds.Select(id => id.ToString()).ToList();
        if (ids.Count == 0) return;
        await _hubContext.Clients.Users(ids).SendAsync("TaskChanged", change);
    }
}
