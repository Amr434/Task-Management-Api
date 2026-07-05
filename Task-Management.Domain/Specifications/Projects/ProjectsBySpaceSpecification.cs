using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Projects;

public class ProjectsBySpaceSpecification : BaseSpecification<Project>
{
    public ProjectsBySpaceSpecification(int spaceId) : base(p => p.SpaceId == spaceId)
    {
    }
}
