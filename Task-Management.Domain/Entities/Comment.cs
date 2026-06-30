namespace Task_Management.Domain.Entities;

public class Comment : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}
