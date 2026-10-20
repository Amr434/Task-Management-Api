namespace Task_Management.Application.Features.Tasks.DTOs;

// Live notification payload: one history entry plus enough about the task to
// title the pop-up and open the task when it's clicked.
public class TaskChangeDto
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public TaskActivityDto Activity { get; set; } = new();
}
