using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Auth.Commands;
using Task_Management.Application.Features.Auth.DTOs;
using Task_Management.Application.Features.Auth.Queries;
using Task_Management.Application.Features.Users.DTOs;

namespace Task_Management.Api.Controllers;

public class AuthController : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
    {
        var result = await Mediator.Send(new LoginCommand(dto));
        // 401 (not the default 400) so the frontend interceptor can react uniformly.
        if (result.IsFailure) return Unauthorized(result.Error);
        return Ok(result.Value);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshRequestDto dto)
    {
        var result = await Mediator.Send(new RefreshTokenCommand(dto.RefreshToken));
        if (result.IsFailure) return Unauthorized(result.Error);
        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout(RefreshRequestDto dto)
    {
        await Mediator.Send(new LogoutCommand(dto.RefreshToken));
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(CurrentUserId));
        return HandleResult(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterUserDto dto)
    {
        var result = await Mediator.Send(new RegisterUserCommand(dto));
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var result = await Mediator.Send(new ChangePasswordCommand(CurrentUserId, dto));
        if (result.IsFailure) return BadRequest(result.Error);
        return Ok();
    }

}
