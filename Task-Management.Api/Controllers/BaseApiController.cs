using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Task_Management.Api.Controllers;

// Every endpoint requires a valid JWT unless explicitly marked [AllowAnonymous]
// (login/refresh in AuthController).
[Authorize]
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private IMediator? _mediator;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected ActionResult HandleResult<T>(Task_Management.Domain.Shared.Result<T> result)
    {
        if (result == null) return NotFound();
        if (result.IsSuccess && result.Value != null) return Ok(result.Value);
        if (result.IsSuccess && result.Value == null) return NotFound();
        
        // Customize based on your needs, e.g., if Error is "NotFound", return 404, else 400.
        if (result.Error.Code.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(result.Error);
        }

        return BadRequest(result.Error);
    }
}
