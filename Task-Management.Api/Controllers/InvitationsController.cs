using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Invitations.Commands;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Application.Features.Invitations.Queries;

namespace Task_Management.Api.Controllers;

public class InvitationsController : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<InvitationDto>> CreateInvitation([FromBody] CreateInvitationDto dto)
    {
        var command = new CreateInvitationCommand(CurrentUserId, dto);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<InvitationDto>>> GetPendingInvitations()
    {
        try 
        {
            var query = new GetPendingInvitationsQuery(CurrentUserId);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EXCEPTION IN GetPendingInvitations: {ex}");
            throw;
        }
    }

    [HttpPost("{id}/respond")]
    public async Task<ActionResult<InvitationDto>> RespondToInvitation(int id, [FromBody] RespondToInvitationRequest request)
    {
        var command = new RespondToInvitationCommand(CurrentUserId, id, request.Accept);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}

public class RespondToInvitationRequest
{
    public bool Accept { get; set; }
}
