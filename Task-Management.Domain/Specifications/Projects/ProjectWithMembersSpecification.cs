using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Projects;

public class ProjectWithMembersSpecification : BaseSpecification<Project>
{
    public ProjectWithMembersSpecification(int projectId) : base(p => p.Id == projectId)
    {
        AddInclude(p => p.Members);
    }
}
