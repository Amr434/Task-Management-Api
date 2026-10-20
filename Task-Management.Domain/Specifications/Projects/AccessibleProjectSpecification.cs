using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Projects;

// A project is accessible via the parent space (owner/member) or via a
// direct project-level share. Null result = no access.
public class AccessibleProjectSpecification : BaseSpecification<Project>
{
    public AccessibleProjectSpecification(int projectId, int userId)
        : base(p => p.Id == projectId && (
            p.Members.Any(m => m.Id == userId) ||
            p.Space!.OwnerId == userId ||
            p.Space!.Members.Any(m => m.Id == userId)))
    {
        AddInclude(p => p.Members);
    }
}
