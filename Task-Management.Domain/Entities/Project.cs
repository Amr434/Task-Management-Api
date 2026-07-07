namespace Task_Management.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int SpaceId { get; set; }
    public Space? Space { get; set; }

    // Navigation properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
