using Task_Management.Domain.Enums;

namespace Task_Management.Domain.Entities;

// One entry in a user's notification list (the bell). The payload is the same
// JSON the live pop-up carries, so the apps render both the same way and in
// the user's language.
public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string Payload { get; set; } = "{}";

    // Set when this notification is worth an email to this user; which email
    // setting controls it. Null = list only.
    public EmailCategory? EmailCategory { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
