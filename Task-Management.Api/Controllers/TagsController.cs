using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Tags.Commands;
using Task_Management.Application.Features.Tags.DTOs;
using Task_Management.Application.Features.Tags.Queries;

namespace Task_Management.Api.Controllers;

public class TagsController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetTags()
    {
        var result = await Mediator.Send(new GetAllTagsQuery());
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto createTagDto)
    {
        var command = new CreateTagCommand(createTagDto);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
