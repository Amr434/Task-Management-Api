using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Spaces;

public class SpaceWithMembersSpecification : BaseSpecification<Space>
{
    public SpaceWithMembersSpecification(int spaceId) : base(s => s.Id == spaceId)
    {
        AddInclude(s => s.Members);
    }
}
