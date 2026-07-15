using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Projects;

namespace Task_Management.Application.Features.Comments;

// Shared access rules for comment handlers. A user may read/write comments
// on a task iff they participate in its project (space owner, space member,
// or project-level share) — the same rule used everywhere else.
internal static class CommentAccess
{
    public static async Task<bool> CanAccessProject(IUnitOfWork unitOfWork, int projectId, int userId)
    {
        var project = await unitOfWork.Repository<Project>()
            .GetEntityWithSpec(new AccessibleProjectSpecification(projectId, userId));
        return project is not null;
    }
}
