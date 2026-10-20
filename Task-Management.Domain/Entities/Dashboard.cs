namespace Task_Management.Domain.Entities;

public class Dashboard : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int SpaceId { get; set; }
    public Space? Space { get; set; }
    public int OwnerId { get; set; }
    public User? Owner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastViewedAt { get; set; }
}
