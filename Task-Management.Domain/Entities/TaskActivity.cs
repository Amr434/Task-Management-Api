using Task_Management.Domain.Enums;

namespace Task_Management.Domain.Entities;

// One entry in a task's history: who changed what, from which value to which.
// Old/New hold display-ready text (a status name, a person's name, a date) so
// the history stays readable even after the referenced tag or user changes.
public class TaskActivity : BaseEntity
{
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public TaskActivityType Type { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
