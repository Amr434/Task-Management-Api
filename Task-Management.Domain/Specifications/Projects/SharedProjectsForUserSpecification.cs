using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Projects;

// All project-level shares for the user, excluding projects whose parent
// space they can already see (those show under the space itself).
public class SharedProjectsForUserSpecification : BaseSpecification<Project>
{
    public SharedProjectsForUserSpecification(int userId)
        : base(p => p.Members.Any(m => m.Id == userId)
            && p.Space!.OwnerId != userId
            && !p.Space!.Members.Any(m => m.Id == userId))
    {
        AddInclude(p => p.Members);
        AddOrderBy(p => p.Id);
    }
}
