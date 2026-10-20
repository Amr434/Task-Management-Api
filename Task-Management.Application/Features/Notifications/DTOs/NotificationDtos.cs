using System.Text.Json;

namespace Task_Management.Application.Features.Notifications.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public int Type { get; set; } // NotificationType
    // TaskChangeDto, CommentDto, InvitationDto or TaskDueDto, depending on Type.
    public JsonElement Payload { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsRead { get; set; }

    // Same JSON shape as the live SignalR messages (camelCase).
    public static readonly JsonSerializerOptions PayloadJson = new(JsonSerializerDefaults.Web);
}

// Payload of the "due tomorrow" and "overdue" reminders.
public class TaskDueDto
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string DueDate { get; set; } = string.Empty; // yyyy-MM-dd
}

public class NotificationSettingsDto
{
    public int EmailMode { get; set; }     // EmailDeliveryMode: Instant=0, DailyDigest=1, Off=2
    public int EmailSummary { get; set; }  // SummaryFrequency: Off=0, Daily=1, Weekly=2

    // One switch per EmailCategory.
    public bool EmailAssignments { get; set; } = true;
    public bool EmailTaskUpdates { get; set; } = true;
    public bool EmailComments { get; set; } = true;
    public bool EmailMentions { get; set; } = true;
    public bool EmailInvitations { get; set; } = true;
    public bool EmailDueReminders { get; set; } = true;
}
