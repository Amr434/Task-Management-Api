using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Spaces;

// Spaces the user can see: ones they own or were accepted into.
public class SpacesForUserSpecification : BaseSpecification<Space>
{
    public SpacesForUserSpecification(int userId)
        : base(s => s.OwnerId == userId || s.Members.Any(m => m.Id == userId))
    {
        AddInclude(s => s.Members);
        AddOrderBy(s => s.Id);
    }
}
