using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments;

internal static class CommentReads
{
    // Saves a "read" row for every unread reply matching the spec and
    // returns how many were marked.
    public static async Task<int> MarkAsync(IUnitOfWork unitOfWork, int userId, UnreadRepliesForUserSpecification spec)
    {
        var unread = await unitOfWork.Repository<Comment>().ListAsync(spec);
        if (unread.Count == 0)
        {
            return 0;
        }

        var reads = unitOfWork.Repository<CommentRead>();
        foreach (var comment in unread)
        {
            reads.Add(new CommentRead { CommentId = comment.Id, UserId = userId, ReadAt = DateTime.UtcNow });
        }

        try
        {
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            // Two requests (e.g. web and mobile, or a poll and a live update)
            // marked the same reply at the same moment: the unique index on
            // (CommentId, UserId) rejects the second one. The reply is read
            // either way, so there's nothing to report.
        }
        return unread.Count;
    }
}
