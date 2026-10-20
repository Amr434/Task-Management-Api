using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

public class TasksInSpaceSpecification : BaseSpecification<TaskItem>
{
    public TasksInSpaceSpecification(int spaceId)
        : base(t => t.Project != null && t.Project.SpaceId == spaceId)
    {
        AddInclude(t => t.Assignees);
        AddInclude(t => t.Project!);
    }
}
