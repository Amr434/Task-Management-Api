using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Comments;

// All comments of one task, oldest first, with the people involved.
public class CommentsByTaskSpecification : BaseSpecification<Comment>
{
    public CommentsByTaskSpecification(int taskId) : base(c => c.TaskItemId == taskId)
    {
        AddInclude(c => c.User!);
        AddInclude(c => c.AssignedTo!);
        AddInclude(c => c.ResolvedBy!);
        AddOrderBy(c => c.CreatedAt);
    }
}
