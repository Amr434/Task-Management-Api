using Task_Management.Domain.Enums;

namespace Task_Management.Application.Features.Tasks.DTOs;

public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public PriorityLevel Priority { get; set; }
    public int Order { get; set; }
    public int ListId { get; set; }
    public int? ParentTaskId { get; set; }

    public ICollection<Task_Management.Application.Features.Tags.DTOs.TagDto> Tags { get; set; } = new List<Task_Management.Application.Features.Tags.DTOs.TagDto>();
}
