using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Spaces;

// Spaces the user can see: ones they own, were accepted into, or that
// contain at least one project shared with them directly (project-level
// invite) — the sidebar nests projects under spaces, so the parent space
// must be listed for a shared project to be reachable.
public class SpacesForUserSpecification : BaseSpecification<Space>
{
    public SpacesForUserSpecification(int userId)
        : base(s => !s.IsPersonal && (
            s.OwnerId == userId
            || s.Members.Any(m => m.Id == userId)
            || s.Projects.Any(p => p.Members.Any(m => m.Id == userId))))
    {
        AddInclude(s => s.Members);
        AddOrderBy(s => s.Id);
    }
}
