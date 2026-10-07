using System.Text.Json;
using Task_Management.Application.Features.Notifications.DTOs;
using Task_Management.Domain.Entities;

namespace Task_Management.Application.Features.Notifications;

public static class NotificationMapping
{
    public static NotificationDto ToDto(Notification n)
    {
        JsonElement payload;
        try
        {
            payload = JsonDocument.Parse(n.Payload).RootElement.Clone();
        }
        catch (JsonException)
        {
            payload = JsonDocument.Parse("{}").RootElement.Clone();
        }

        return new NotificationDto
        {
            Id = n.Id,
            Type = (int)n.Type,
            Payload = payload,
            CreatedAtUtc = n.CreatedAtUtc,
            IsRead = n.ReadAtUtc != null,
        };
    }
}
