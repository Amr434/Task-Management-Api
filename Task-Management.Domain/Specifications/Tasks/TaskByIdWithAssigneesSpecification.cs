using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

public class TaskByIdWithAssigneesSpecification : BaseSpecification<TaskItem>
{
    public TaskByIdWithAssigneesSpecification(int taskId) : base(t => t.Id == taskId)
    {
        AddInclude(t => t.Assignees);
    }
}
