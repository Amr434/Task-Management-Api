using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Comments;

// Comments assigned to the user (their action items), newest first, with
// task/project/space context for the "Assigned Comments" view.
public class AssignedCommentsForUserSpecification : BaseSpecification<Comment>
{
    public AssignedCommentsForUserSpecification(int userId) : base(c => c.AssignedToId == userId)
    {
        AddInclude(c => c.User!);
        AddInclude(c => c.AssignedTo!);
        AddInclude(c => c.ResolvedBy!);
        AddInclude(c => c.TaskItem!);
        AddInclude($"{nameof(Comment.TaskItem)}.{nameof(TaskItem.Project)}");
        AddInclude($"{nameof(Comment.TaskItem)}.{nameof(TaskItem.Project)}.{nameof(Project.Space)}");
        AddOrderByDescending(c => c.CreatedAt);
    }
}
