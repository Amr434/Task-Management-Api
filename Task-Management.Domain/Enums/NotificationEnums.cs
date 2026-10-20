namespace Task_Management.Domain.Enums;

// What a saved notification is about; decides the shape of its payload.
// Values are persisted, so only append.
public enum NotificationType
{
    TaskChanged = 0,          // payload: TaskChangeDto
    CommentAdded = 1,         // payload: CommentDto
    Mentioned = 2,            // payload: CommentDto
    InvitationReceived = 3,   // payload: InvitationDto
    InvitationResponded = 4,  // payload: InvitationDto
    TaskDueSoon = 5,          // payload: TaskDueDto
    TaskOverdue = 6           // payload: TaskDueDto
}

// The kinds of email a user can turn off one by one. Values are persisted
// (as bits of User.MutedEmailCategories), so only append.
public enum EmailCategory
{
    Assignments = 0,   // assigned to / removed from a task
    TaskUpdates = 1,   // status or due date changed
    Comments = 2,      // a comment was assigned to you
    Mentions = 3,      // someone @mentioned you
    Invitations = 4,   // invited, or your invitation was answered
    DueReminders = 5   // due tomorrow / overdue
}

public enum EmailDeliveryMode
{
    Instant = 0,       // one email per event
    DailyDigest = 1,   // one email a day listing the day's events
    Off = 2
}

// The "You have 5 tasks due this week" overview email.
public enum SummaryFrequency
{
    Off = 0,
    Daily = 1,
    Weekly = 2
}
