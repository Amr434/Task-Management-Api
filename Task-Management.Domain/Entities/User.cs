namespace Task_Management.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Identity or auth provider ID
    public string ExternalId { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Space> Spaces { get; set; } = new List<Space>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
