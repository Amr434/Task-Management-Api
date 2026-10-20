using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Comments;

// Replies the user hasn't opened yet: the same comments as the Replies feed
// (see RepliesForUserSpecification), minus the user's own comments and minus
// the ones they've already read. Pass a taskId to limit it to one task
// (used when the user opens that task), or a commentId for a single comment.
// Used for the unread badge (CountAsync) and for marking replies as read.
public class UnreadRepliesForUserSpecification : BaseSpecification<Comment>
{
    public UnreadRepliesForUserSpecification(int userId, int? taskId = null, int? commentId = null)
        : base(c =>
            (taskId == null || c.TaskItemId == taskId) &&
            (commentId == null || c.Id == commentId) &&
            c.UserId != userId &&
            !c.Reads.Any(r => r.UserId == userId) &&
            (c.AssignedToId == userId ||
             c.TaskItem!.Project!.Space!.OwnerId == userId ||
             c.TaskItem.Project.Space.Members.Any(m => m.Id == userId) ||
             c.TaskItem.Project.Members.Any(m => m.Id == userId)))
    {
    }
}
