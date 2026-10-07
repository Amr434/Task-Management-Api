using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Tasks;

public class AssignedTasksWithDetailsSpecification : BaseSpecification<TaskItem>
{
    public AssignedTasksWithDetailsSpecification(int userId) : base(t => 
        t.Assignees.Any(a => a.Id == userId) &&
        (t.Project!.Space!.OwnerId == userId || 
         t.Project.Space.Members.Any(m => m.Id == userId) || 
         t.Project.Members.Any(m => m.Id == userId)))
    {
        AddInclude(t => t.Tags);
        AddInclude(t => t.Assignees);
        AddInclude(t => t.Comments);
        AddInclude(t => t.Attachments);
        AddInclude(t => t.Project!); // Include Project to get Project Name
        AddInclude($"{nameof(TaskItem.Project)}.{nameof(Project.Space)}"); // Include Space to get Space Name

        // Subtasks of assigned tasks (with their own tags/assignees), so the
        // "Assigned to me" view can render the subtask tree even when the
        // subtasks themselves are not assigned to the user.
        AddInclude(t => t.SubTasks);
        AddInclude($"{nameof(TaskItem.SubTasks)}.{nameof(TaskItem.Tags)}");
        AddInclude($"{nameof(TaskItem.SubTasks)}.{nameof(TaskItem.Assignees)}");
        
        // Order by DueDate
        AddOrderBy(t => t.DueDate ?? DateTime.MaxValue);
    }
}
