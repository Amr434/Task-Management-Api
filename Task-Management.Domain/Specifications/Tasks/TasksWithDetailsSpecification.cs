using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

public class TasksWithDetailsSpecification : BaseSpecification<TaskItem>
{
    public TasksWithDetailsSpecification(int listId) : base(t => t.ListId == listId)
    {
        AddInclude(t => t.Tags);
        AddInclude(t => t.Assignees);
        AddInclude(t => t.Comments);
        AddInclude(t => t.Attachments);
        
        // Order by Order integer field
        AddOrderBy(t => t.Order);
    }
}
