using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Infrastructure.Notifications;

namespace Task_Management.Api.Services;

// Each notification goes out live as a pop-up (SignalR) and is saved to the
// user's notification list, which also emails it (see NotificationCenter).
// The channels run independently: one failing doesn't stop the other.
internal static class NotifyAll
{
    public static async Task RunAsync(ILogger logger, params Func<Task>[] channels)
    {
        foreach (var channel in channels)
        {
            try
            {
                await channel();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "A notification channel failed.");
            }
        }
    }
}

public class CompositeTaskNotifier : ITaskNotifier
{
    private readonly SignalRTaskNotifier _live;
    private readonly InboxTaskNotifier _inbox;
    private readonly ILogger<CompositeTaskNotifier> _logger;

    public CompositeTaskNotifier(SignalRTaskNotifier live, InboxTaskNotifier inbox, ILogger<CompositeTaskNotifier> logger)
    {
        _live = live;
        _inbox = inbox;
        _logger = logger;
    }

    public Task TaskChangedAsync(IEnumerable<int> recipientUserIds, TaskChangeDto change)
    {
        var ids = recipientUserIds.ToList();
        return NotifyAll.RunAsync(_logger,
            () => _live.TaskChangedAsync(ids, change),
            () => _inbox.TaskChangedAsync(ids, change));
    }
}

public class CompositeInvitationNotifier : IInvitationNotifier
{
    private readonly SignalRInvitationNotifier _live;
    private readonly InboxInvitationNotifier _inbox;
    private readonly ILogger<CompositeInvitationNotifier> _logger;

    public CompositeInvitationNotifier(SignalRInvitationNotifier live, InboxInvitationNotifier inbox, ILogger<CompositeInvitationNotifier> logger)
    {
        _live = live;
        _inbox = inbox;
        _logger = logger;
    }

    public Task InvitationReceivedAsync(int inviteeUserId, InvitationDto invitation) =>
        NotifyAll.RunAsync(_logger,
            () => _live.InvitationReceivedAsync(inviteeUserId, invitation),
            () => _inbox.InvitationReceivedAsync(inviteeUserId, invitation));

    public Task InvitationRespondedAsync(int inviterUserId, InvitationDto invitation) =>
        NotifyAll.RunAsync(_logger,
            () => _live.InvitationRespondedAsync(inviterUserId, invitation),
            () => _inbox.InvitationRespondedAsync(inviterUserId, invitation));
}

public class CompositeCommentNotifier : ICommentNotifier
{
    private readonly SignalRCommentNotifier _live;
    private readonly InboxCommentNotifier _inbox;
    private readonly ILogger<CompositeCommentNotifier> _logger;

    public CompositeCommentNotifier(SignalRCommentNotifier live, InboxCommentNotifier inbox, ILogger<CompositeCommentNotifier> logger)
    {
        _live = live;
        _inbox = inbox;
        _logger = logger;
    }

    public Task CommentAddedAsync(IEnumerable<int> recipientUserIds, CommentDto comment, IReadOnlyCollection<int> mentionedUserIds)
    {
        var ids = recipientUserIds.ToList();
        return NotifyAll.RunAsync(_logger,
            () => _live.CommentAddedAsync(ids, comment, mentionedUserIds),
            () => _inbox.CommentAddedAsync(ids, comment, mentionedUserIds));
    }
}
