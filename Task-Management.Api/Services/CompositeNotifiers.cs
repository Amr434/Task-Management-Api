using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Infrastructure.Email;

namespace Task_Management.Api.Services;

// Each notification goes out live (SignalR) and by email. The channels run
// independently: one failing doesn't stop the other.
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
    private readonly EmailTaskNotifier _email;
    private readonly ILogger<CompositeTaskNotifier> _logger;

    public CompositeTaskNotifier(SignalRTaskNotifier live, EmailTaskNotifier email, ILogger<CompositeTaskNotifier> logger)
    {
        _live = live;
        _email = email;
        _logger = logger;
    }

    public Task TaskChangedAsync(IEnumerable<int> recipientUserIds, TaskChangeDto change)
    {
        var ids = recipientUserIds.ToList();
        return NotifyAll.RunAsync(_logger,
            () => _live.TaskChangedAsync(ids, change),
            () => _email.TaskChangedAsync(ids, change));
    }
}

public class CompositeInvitationNotifier : IInvitationNotifier
{
    private readonly SignalRInvitationNotifier _live;
    private readonly EmailInvitationNotifier _email;
    private readonly ILogger<CompositeInvitationNotifier> _logger;

    public CompositeInvitationNotifier(SignalRInvitationNotifier live, EmailInvitationNotifier email, ILogger<CompositeInvitationNotifier> logger)
    {
        _live = live;
        _email = email;
        _logger = logger;
    }

    public Task InvitationReceivedAsync(int inviteeUserId, InvitationDto invitation) =>
        NotifyAll.RunAsync(_logger,
            () => _live.InvitationReceivedAsync(inviteeUserId, invitation),
            () => _email.InvitationReceivedAsync(inviteeUserId, invitation));

    public Task InvitationRespondedAsync(int inviterUserId, InvitationDto invitation) =>
        NotifyAll.RunAsync(_logger,
            () => _live.InvitationRespondedAsync(inviterUserId, invitation),
            () => _email.InvitationRespondedAsync(inviterUserId, invitation));
}

public class CompositeCommentNotifier : ICommentNotifier
{
    private readonly SignalRCommentNotifier _live;
    private readonly EmailCommentNotifier _email;
    private readonly ILogger<CompositeCommentNotifier> _logger;

    public CompositeCommentNotifier(SignalRCommentNotifier live, EmailCommentNotifier email, ILogger<CompositeCommentNotifier> logger)
    {
        _live = live;
        _email = email;
        _logger = logger;
    }

    public Task CommentAddedAsync(IEnumerable<int> recipientUserIds, CommentDto comment)
    {
        var ids = recipientUserIds.ToList();
        return NotifyAll.RunAsync(_logger,
            () => _live.CommentAddedAsync(ids, comment),
            () => _email.CommentAddedAsync(ids, comment));
    }
}
