namespace Task_Management.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int SpaceId { get; set; }

    // Extra board columns beyond the built-in ToDo/InProgress/Complete, as a
    // JSON array of { id, name, color }. Tasks in one carry Status = id (>= 3).
    public string? CustomStatuses { get; set; }
    public Space? Space { get; set; }

    // Navigation properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    // Project-level shares: users who can access just this project
    // without being members of the parent space.
    public ICollection<User> Members { get; set; } = new List<User>();
}
