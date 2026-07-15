using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Comments;

// One comment with the people involved and its task (for access checks
// and for mapping task context onto the DTO).
public class CommentByIdWithDetailsSpecification : BaseSpecification<Comment>
{
    public CommentByIdWithDetailsSpecification(int commentId) : base(c => c.Id == commentId)
    {
        AddInclude(c => c.User!);
        AddInclude(c => c.AssignedTo!);
        AddInclude(c => c.ResolvedBy!);
        AddInclude(c => c.TaskItem!);
    }
}
