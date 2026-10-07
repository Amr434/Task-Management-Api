using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Enums;

namespace Task_Management.Application.Features.Tasks.DTOs;

public class TaskActivityDto
{
    public int Id { get; set; }
    public TaskActivityType Type { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDto? User { get; set; }
}
