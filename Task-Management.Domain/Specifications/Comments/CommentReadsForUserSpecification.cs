using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Comments;

// Which of these comments the user has already read.
public class CommentReadsForUserSpecification : BaseSpecification<CommentRead>
{
    public CommentReadsForUserSpecification(int userId, IEnumerable<int> commentIds)
        : base(r => r.UserId == userId && commentIds.Contains(r.CommentId))
    {
    }
}
