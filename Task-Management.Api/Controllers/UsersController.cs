using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Users.Commands;
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

    // ---- Profile picture (every user, their own) ----

    [HttpPost("me/avatar")]
    [RequestSizeLimit(6_000_000)] // 5 MB picture + form overhead
    public async Task<ActionResult<UserDto>> UploadAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new Task_Management.Domain.Shared.Error("Avatar.Empty", "No picture was provided."));
        }
        await using var stream = file.OpenReadStream();
        var result = await Mediator.Send(new UploadAvatarCommand(CurrentUserId, file.ContentType, file.Length, stream));
        return HandleResult(result);
    }

    [HttpDelete("me/avatar")]
    public async Task<ActionResult<UserDto>> RemoveAvatar()
    {
        var result = await Mediator.Send(new RemoveAvatarCommand(CurrentUserId));
        return HandleResult(result);
    }

    // Public so plain <img> tags (which can't send the login token) can load it.
    // The URL carries ?v=<unique id>, so it can be cached for a long time.
    [AllowAnonymous]
    [HttpGet("{id}/avatar")]
    [ResponseCache(Duration = 604800)]
    public async Task<IActionResult> GetAvatar(int id)
    {
        var result = await Mediator.Send(new GetAvatarQuery(id));
        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }
        return File(result.Value.Content, result.Value.ContentType);
    }

    // ---- User management (Super Admin: Admins + Members, Admin: Members only) ----
    // The [Authorize] roles are a first gate; each handler re-checks the
    // current role from the database and the "who may manage whom" rules.

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("manage")]
    public async Task<ActionResult<IEnumerable<ManagedUserDto>>> GetManagedUsers()
    {
        var result = await Mediator.Send(new GetManagedUsersQuery(CurrentUserId));
        return HandleResult(result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id}/role")]
    public async Task<ActionResult<ManagedUserDto>> UpdateRole(int id, UpdateUserRoleDto dto)
    {
        var result = await Mediator.Send(new UpdateUserRoleCommand(CurrentUserId, id, dto.Role));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id}/status")]
    public async Task<ActionResult<ManagedUserDto>> SetStatus(int id, SetUserStatusDto dto)
    {
        var result = await Mediator.Send(new SetUserStatusCommand(CurrentUserId, id, dto.IsActive));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<ManagedUserDto>> ResetPassword(int id, ResetUserPasswordDto dto)
    {
        var result = await Mediator.Send(new ResetUserPasswordCommand(CurrentUserId, id, dto.NewPassword));
        return HandleResult(result);
    }
}
