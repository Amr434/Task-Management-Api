using Task_Management.Domain.Enums;

namespace Task_Management.Application.Features.Tasks.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public PriorityLevel Priority { get; set; }
    public int Order { get; set; }
    public int ListId { get; set; }
    public int? ParentTaskId { get; set; }
}
