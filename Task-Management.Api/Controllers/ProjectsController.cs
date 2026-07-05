using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Projects.Commands;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Application.Features.Projects.Queries;

namespace Task_Management.Api.Controllers;

public class ProjectsController : BaseApiController
{
    [HttpGet("space/{spaceId}")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects(int spaceId)
    {
        var result = await Mediator.Send(new GetProjectsBySpaceQuery(spaceId));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
    {
        var command = new CreateProjectCommand(createProjectDto);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        var command = new DeleteProjectCommand(id);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }
}
