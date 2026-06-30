namespace Task_Management.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int WorkspaceId { get; set; }
    public Workspace? Workspace { get; set; }

    // Navigation properties
    public ICollection<List> Lists { get; set; } = new List<List>();
}
