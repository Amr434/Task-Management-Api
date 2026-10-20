using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Spaces;

// The user's hidden Personal List space (at most one per user).
public class PersonalSpaceForUserSpecification : BaseSpecification<Space>
{
    public PersonalSpaceForUserSpecification(int userId)
        : base(s => s.IsPersonal && s.OwnerId == userId)
    {
        AddInclude(s => s.Projects);
    }
}
