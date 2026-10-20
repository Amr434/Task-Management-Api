using Microsoft.AspNetCore.Mvc;
using Task_Management.Application.Features.Notifications.Commands;
using Task_Management.Application.Features.Notifications.DTOs;
using Task_Management.Application.Features.Notifications.Queries;

namespace Task_Management.Api.Controllers;

// The current user's notification list (the bell) and their email settings.
public class NotificationsController : BaseApiController
{
    // Newest first. For the next page pass the last id you got as beforeId.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromQuery] int? beforeId, [FromQuery] int take = 20)
    {
        var result = await Mediator.Send(new GetNotificationsQuery(CurrentUserId, beforeId, take));
        return HandleResult(result);
    }

    // The number on the bell.
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var result = await Mediator.Send(new GetUnreadNotificationCountQuery(CurrentUserId));
        return HandleResult(result);
    }

    [HttpPost("{id}/read")]
    public async Task<ActionResult<int>> MarkRead(int id)
    {
        var result = await Mediator.Send(new MarkNotificationsReadCommand(CurrentUserId, id));
        return HandleResult(result);
    }

    // "Mark all as read".
    [HttpPost("read")]
    public async Task<ActionResult<int>> MarkAllRead()
    {
        var result = await Mediator.Send(new MarkNotificationsReadCommand(CurrentUserId));
        return HandleResult(result);
    }

    [HttpGet("settings")]
    public async Task<ActionResult<NotificationSettingsDto>> GetSettings()
    {
        var result = await Mediator.Send(new GetNotificationSettingsQuery(CurrentUserId));
        return HandleResult(result);
    }

    [HttpPut("settings")]
    public async Task<ActionResult<NotificationSettingsDto>> UpdateSettings(NotificationSettingsDto dto)
    {
        var result = await Mediator.Send(new UpdateNotificationSettingsCommand(CurrentUserId, dto));
        return HandleResult(result);
    }
}
