using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

// Tasks the user can open (same access rule as projects: space owner/member
// or a direct project share) whose number matches taskId or whose title
// contains the search text.
public class SearchAccessibleTasksSpecification : BaseSpecification<TaskItem>
{
    public SearchAccessibleTasksSpecification(int userId, string text, int? taskId, int take)
        : base(t =>
            (t.Project!.Space!.OwnerId == userId ||
             t.Project.Space.Members.Any(m => m.Id == userId) ||
             t.Project.Members.Any(m => m.Id == userId)) &&
            ((taskId != null && t.Id == taskId) || t.Title.Contains(text)))
    {
        AddInclude(t => t.Assignees);
        AddInclude(t => t.Tags);
        AddInclude(t => t.Project!);
        AddInclude($"{nameof(TaskItem.Project)}.{nameof(Project.Space)}");
        // The exact task number first (so "2" always finds task #2 even when many
        // titles contain a 2), then newest first.
        AddOrderByDescending(t => taskId != null && t.Id == taskId ? int.MaxValue : t.Id);
        ApplyPaging(0, take);
    }
}
