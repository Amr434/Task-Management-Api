using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Workspaces.Commands;
using Task_Management.Application.Features.Workspaces.DTOs;
using Task_Management.Application.Features.Workspaces.Queries;

namespace Task_Management.Api.Controllers;

public class WorkspacesController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspaceDto>>> GetWorkspaces()
    {
        var result = await Mediator.Send(new GetWorkspacesQuery());
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkspaceDto>> GetWorkspace(int id)
    {
        var result = await Mediator.Send(new GetWorkspaceByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<WorkspaceDto>> CreateWorkspace([FromBody] CreateWorkspaceDto createWorkspaceDto)
    {
        var command = new CreateWorkspaceCommand(createWorkspaceDto);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }
}
