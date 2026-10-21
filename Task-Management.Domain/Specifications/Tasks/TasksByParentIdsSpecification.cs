using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

// Direct subtasks of the given tasks.
public class TasksByParentIdsSpecification : BaseSpecification<TaskItem>
{
    public TasksByParentIdsSpecification(IReadOnlyCollection<int> parentIds)
        : base(t => t.ParentTaskId != null && parentIds.Contains(t.ParentTaskId.Value))
    {
    }
}
