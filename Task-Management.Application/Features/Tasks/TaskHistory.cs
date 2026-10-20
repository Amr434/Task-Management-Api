using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Application.Features.Tasks;

// Builds TaskActivity entries. Entries are attached through task.Activities so
// they are saved in the same CompleteAsync as the change they describe — and
// so a brand-new task can log "Created" before it has an Id.
//
// Status and priority are stored as enum names (e.g. "InProgress") so clients
// can localise them; dates as yyyy-MM-dd.
internal static class TaskHistory
{
    public static void Record(TaskItem task, int userId, TaskActivityType type, string? oldValue = null, string? newValue = null)
    {
        task.Activities.Add(new TaskActivity
        {
            UserId = userId,
            Type = type,
            OldValue = Truncate(oldValue),
            NewValue = Truncate(newValue),
        });
    }

    // For changes to things that hang off a task (comments, attachments) where
    // the task itself isn't loaded: add the entry straight to its repository,
    // still before the CompleteAsync that saves the change.
    public static void Record(IUnitOfWork unitOfWork, int taskId, int userId, TaskActivityType type, string? oldValue = null, string? newValue = null)
    {
        unitOfWork.Repository<TaskActivity>().Add(new TaskActivity
        {
            TaskItemId = taskId,
            UserId = userId,
            Type = type,
            OldValue = Truncate(oldValue),
            NewValue = Truncate(newValue),
        });
    }

    public static string? DateText(DateTime? date) => date?.ToString("yyyy-MM-dd");

    // A comment's text as a one-line excerpt for the history list.
    public static string Excerpt(string text)
    {
        var line = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return line.Length > ExcerptLength ? line[..ExcerptLength].TrimEnd() + "…" : line;
    }

    private const int ExcerptLength = 80;

    public static string PersonName(User user)
    {
        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? user.Email : name;
    }

    private static string? Truncate(string? value) =>
        value is { Length: > 500 } ? value[..500] : value;
}
