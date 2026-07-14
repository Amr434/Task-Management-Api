using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Projects;

// Projects inside a space that were shared directly with the user
// (used when the user lacks space-level access).
public class SharedProjectsInSpaceSpecification : BaseSpecification<Project>
{
    public SharedProjectsInSpaceSpecification(int spaceId, int userId)
        : base(p => p.SpaceId == spaceId && p.Members.Any(m => m.Id == userId))
    {
        AddOrderBy(p => p.Id);
    }
}
