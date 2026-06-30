using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Lists.Commands;
using Task_Management.Application.Features.Lists.DTOs;
using Task_Management.Application.Features.Lists.Queries;

namespace Task_Management.Api.Controllers;

public class ListsController : BaseApiController
{
    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<ListDto>>> GetLists(int projectId)
    {
        var result = await Mediator.Send(new GetListsByProjectQuery(projectId));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ListDto>> CreateList([FromBody] CreateListDto createListDto)
    {
        var command = new CreateListCommand(createListDto);
        var result = await Mediator.Send(command);
        
        return HandleResult(result);
    }
}
