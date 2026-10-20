namespace Task_Management.Domain.Entities;

public class Space : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }

    // Creator/owner. Only the owner and accepted Members can see the space.
    // Nullable for rows created before sharing existed (seed backfills them).
    public int? OwnerId { get; set; }
    public User? Owner { get; set; }

    // Hidden per-user space backing the "Personal List" (ClickUp-style):
    // lazily created, never listed in the sidebar, never shareable.
    public bool IsPersonal { get; set; }

    // Navigation properties
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<User> Members { get; set; } = new List<User>();
}
