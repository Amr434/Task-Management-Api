using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Application.Features.Users.Queries;

namespace Task_Management.Api.Controllers;

public class UsersController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var result = await Mediator.Send(new GetAllUsersQuery());
        return HandleResult(result);
    }
}
