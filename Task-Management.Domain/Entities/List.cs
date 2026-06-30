namespace Task_Management.Domain.Entities;

public class List : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    // Navigation properties
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
