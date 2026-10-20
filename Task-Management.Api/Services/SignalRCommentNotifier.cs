using Microsoft.AspNetCore.SignalR;
using Task_Management.Api.Hubs;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Comments.DTOs;

namespace Task_Management.Api.Services;

// Uses the same hub connection the apps already open for invitations.
public class SignalRCommentNotifier : ICommentNotifier
{
    private readonly IHubContext<InvitationHub> _hubContext;

    public SignalRCommentNotifier(IHubContext<InvitationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // Everyone gets the same pop-up; mentions show up in the bell (see NotificationCenter).
    public async Task CommentAddedAsync(IEnumerable<int> recipientUserIds, CommentDto comment, IReadOnlyCollection<int> mentionedUserIds)
    {
        var ids = recipientUserIds.Select(id => id.ToString()).ToList();
        if (ids.Count == 0) return;
        await _hubContext.Clients.Users(ids).SendAsync("CommentAdded", comment);
    }
}
