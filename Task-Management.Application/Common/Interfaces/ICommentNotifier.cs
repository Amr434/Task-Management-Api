using Task_Management.Application.Features.Comments.DTOs;

namespace Task_Management.Application.Common.Interfaces;

// Implemented in the Api layer with SignalR: tells users' open web/mobile
// sessions that a new comment was written, so they can show a notification.
// No-ops for users who aren't connected. mentionedUserIds (a subset of the
// recipients) were @mentioned in it.
public interface ICommentNotifier
{
    Task CommentAddedAsync(IEnumerable<int> recipientUserIds, CommentDto comment, IReadOnlyCollection<int> mentionedUserIds);
}
