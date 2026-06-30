using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

public class TaskByIdWithTagsSpecification : BaseSpecification<TaskItem>
{
    public TaskByIdWithTagsSpecification(int taskId) : base(t => t.Id == taskId)
    {
        AddInclude(t => t.Tags);
    }
}
