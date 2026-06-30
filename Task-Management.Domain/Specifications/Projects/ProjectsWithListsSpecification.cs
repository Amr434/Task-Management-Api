using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Projects;

public class ProjectsWithListsSpecification : BaseSpecification<Project>
{
    public ProjectsWithListsSpecification(int workspaceId) : base(p => p.WorkspaceId == workspaceId)
    {
        AddInclude(p => p.Lists);
        // Order by Name as a default
        AddOrderBy(p => p.Name);
    }
}
