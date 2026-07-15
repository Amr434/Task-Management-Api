namespace Task_Management.Domain.Entities;

public class Comment : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    // ClickUp-style "assigned comment": an action item for a project
    // participant. Resolved state is carried by ResolvedById/ResolvedAt.
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public int? ResolvedById { get; set; }
    public User? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
