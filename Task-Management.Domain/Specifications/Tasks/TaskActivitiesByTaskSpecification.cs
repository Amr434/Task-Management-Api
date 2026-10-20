using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

// A task's history, newest first, with who made each change.
public class TaskActivitiesByTaskSpecification : BaseSpecification<TaskActivity>
{
    public TaskActivitiesByTaskSpecification(int taskId) : base(a => a.TaskItemId == taskId)
    {
        AddInclude(a => a.User!);
        AddOrderByDescending(a => a.CreatedAt);
    }
}
