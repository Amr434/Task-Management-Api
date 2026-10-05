using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Comments;

// The "Replies" feed, newest first, with task/project/space context:
//  - every comment (anyone's, including the user's own) on any task in a
//    project the user can access: their space (owner/member) or a project
//    shared with them, whether or not they're assigned, and whatever the
//    task's status; and
//  - any comment assigned to the user.
// Because it follows current access, a task nobody else can reach shows its
// comments to you alone, and someone given access later sees the whole
// history, including comments written before they joined.
public class RepliesForUserSpecification : BaseSpecification<Comment>
{
    public RepliesForUserSpecification(int userId, int take)
        : base(c =>
            c.AssignedToId == userId ||
            c.TaskItem!.Project!.Space!.OwnerId == userId ||
            c.TaskItem.Project.Space.Members.Any(m => m.Id == userId) ||
            c.TaskItem.Project.Members.Any(m => m.Id == userId))
    {
        AddInclude(c => c.User!);
        AddInclude(c => c.AssignedTo!);
        AddInclude(c => c.ResolvedBy!);
        AddInclude(c => c.TaskItem!);
        AddInclude($"{nameof(Comment.TaskItem)}.{nameof(TaskItem.Project)}");
        AddInclude($"{nameof(Comment.TaskItem)}.{nameof(TaskItem.Project)}.{nameof(Project.Space)}");
        AddOrderByDescending(c => c.CreatedAt);
        ApplyPaging(0, take);
    }
}
