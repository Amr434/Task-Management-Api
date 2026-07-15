using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Projects.Commands;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Application.Features.Projects.Queries;
using Task_Management.Application.Features.Users.DTOs;

namespace Task_Management.Api.Controllers;

public class ProjectsController : BaseApiController
{
    // The current user's private Personal List project (created on first call).
    [HttpGet("personal")]
    public async Task<ActionResult<ProjectDto>> GetPersonalProject()
    {
        var result = await Mediator.Send(new GetPersonalProjectQuery(CurrentUserId));
        return HandleResult(result);
    }

    [HttpGet("space/{spaceId}")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects(int spaceId)
    {
        var result = await Mediator.Send(new GetProjectsBySpaceQuery(spaceId, CurrentUserId));
        return HandleResult(result);
    }

    // Assignable users for a project: space owner + space members + project shares.
    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetProjectMembers(int id)
    {
        var result = await Mediator.Send(new GetProjectMembersQuery(id, CurrentUserId));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
    {
        var command = new CreateProjectCommand(createProjectDto);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectDto updateProjectDto)
    {
        var command = new UpdateProjectCommand(id, updateProjectDto);
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
