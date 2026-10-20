using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Spaces.Commands;
using Task_Management.Application.Features.Spaces.DTOs;
using Task_Management.Application.Features.Spaces.Queries;

namespace Task_Management.Api.Controllers;

public class SpacesController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpaceDto>>> GetSpaces()
    {
        var result = await Mediator.Send(new GetAllSpacesQuery(CurrentUserId));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<SpaceDto>> CreateSpace([FromBody] CreateSpaceDto createSpaceDto)
    {
        var command = new CreateSpaceCommand(createSpaceDto, CurrentUserId);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SpaceDto>> UpdateSpace(int id, [FromBody] UpdateSpaceDto updateSpaceDto)
    {
        var command = new UpdateSpaceCommand(id, updateSpaceDto, CurrentUserId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSpace(int id)
    {
        var command = new DeleteSpaceCommand(id, CurrentUserId);
        var result = await Mediator.Send(command);

        return HandleResult(result);
    }
}
