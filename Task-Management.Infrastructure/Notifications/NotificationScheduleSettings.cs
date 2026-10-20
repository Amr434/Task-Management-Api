namespace Task_Management.Infrastructure.Notifications;

// "NotificationSchedule" config section: when the daily emails go out.
public class NotificationScheduleSettings
{
    public const string SectionName = "NotificationSchedule";

    // The team's time zone: decides what "today", "tomorrow" and the send
    // hour mean. An IANA or Windows id; empty = the server's own time zone.
    public string TimeZone { get; set; } = string.Empty;

    // Local hour (0-23) after which each day's reminders, digests and
    // summaries are sent.
    public int SendHour { get; set; } = 8;

    // Day the weekly summary goes out.
    public DayOfWeek WeeklySummaryDay { get; set; } = DayOfWeek.Sunday;

    // Read notifications older than this are deleted.
    public int KeepReadNotificationsDays { get; set; } = 90;

    public TimeZoneInfo ResolveTimeZone()
    {
        if (string.IsNullOrWhiteSpace(TimeZone)) return TimeZoneInfo.Local;
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }
}
