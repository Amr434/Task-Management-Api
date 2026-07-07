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
        var result = await Mediator.Send(new GetTasksByProjectQuery(projectId));
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
