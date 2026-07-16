using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Tasks.Commands;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Application.Features.Tasks.Queries;

namespace Task_Management.Api.Controllers;

public class TasksController : BaseApiController
{
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks(int projectId)
    {
        var result = await Mediator.Send(new GetTasksByProjectQuery(projectId, CurrentUserId));
        return HandleResult(result);
    }

    [HttpGet("assigned")]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAssignedTasks()
    {
        var result = await Mediator.Send(new GetAssignedTasksQuery(CurrentUserId));
        return HandleResult(result);
    }

    // ---- Attachments ----

    [HttpGet("{taskId}/attachments")]
    public async Task<ActionResult<IEnumerable<Task_Management.Application.Features.Attachments.DTOs.AttachmentDto>>> GetTaskAttachments(int taskId)
    {
        var result = await Mediator.Send(new Task_Management.Application.Features.Attachments.Queries.GetTaskAttachmentsQuery(taskId, CurrentUserId));
        return HandleResult(result);
    }

    [HttpPost("{taskId}/attachments")]
    [RequestSizeLimit(26_214_400)] // 25 MB
    public async Task<ActionResult<Task_Management.Application.Features.Attachments.DTOs.AttachmentDto>> UploadAttachment(int taskId, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new Task_Management.Domain.Shared.Error("Attachment.Empty", "No file was provided."));
        }
        await using var stream = file.OpenReadStream();
        var command = new Task_Management.Application.Features.Attachments.Commands.UploadAttachmentCommand(
            taskId, CurrentUserId, file.FileName, file.ContentType, file.Length, stream);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    // ---- Comments ----

    [HttpGet("{taskId}/comments")]
    public async Task<ActionResult<IEnumerable<Task_Management.Application.Features.Comments.DTOs.CommentDto>>> GetTaskComments(int taskId)
    {
        var result = await Mediator.Send(new Task_Management.Application.Features.Comments.Queries.GetTaskCommentsQuery(taskId, CurrentUserId));
        return HandleResult(result);
    }

    [HttpPost("{taskId}/comments")]
    public async Task<ActionResult<Task_Management.Application.Features.Comments.DTOs.CommentDto>> CreateComment(int taskId, [FromBody] Task_Management.Application.Features.Comments.DTOs.CreateCommentDto dto)
    {
        var result = await Mediator.Send(new Task_Management.Application.Features.Comments.Commands.CreateCommentCommand(taskId, CurrentUserId, dto));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        var command = new CreateTaskCommand(createTaskDto);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }

    [HttpPost("{taskId}/tags/{tagId}")]
    public async Task<ActionResult> AssignTag(int taskId, int tagId)
    {
        var command = new AssignTagToTaskCommand(taskId, tagId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpDelete("{taskId}/tags/{tagId}")]
    public async Task<ActionResult> RemoveTag(int taskId, int tagId)
    {
        var command = new RemoveTagFromTaskCommand(taskId, tagId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpPost("{taskId}/assignees/{userId}")]
    public async Task<ActionResult> AssignUser(int taskId, int userId)
    {
        var command = new AssignUserToTaskCommand(taskId, userId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpDelete("{taskId}/assignees/{userId}")]
    public async Task<ActionResult> RemoveUser(int taskId, int userId)
    {
        var command = new RemoveUserFromTaskCommand(taskId, userId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItemDto>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        var command = new UpdateTaskCommand(id, updateTaskDto);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTask(int id)
    {
        var command = new DeleteTaskCommand(id);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }
}
