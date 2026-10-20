using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Spaces;

// Resolves a single space only if the user owns it or is a member —
// a null result doubles as the access check.
public class AccessibleSpaceSpecification : BaseSpecification<Space>
{
    public AccessibleSpaceSpecification(int spaceId, int userId)
        : base(s => s.Id == spaceId && (s.OwnerId == userId || s.Members.Any(m => m.Id == userId)))
    {
    }
}
