using AutoMapper;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks;

// Turns saved task-history entries into live pop-ups (see
// TaskChangeNotificationBehavior, which calls this after every command).
internal static class TaskNotifications
{
    /// <summary>
    /// Sends each entry to its task's assignees plus <paramref name="alsoNotify"/>,
    /// never to the person who made the change. A failure here must not undo
    /// the saved change, so it's swallowed.
    /// </summary>
    public static async Task SendAsync(
        IUnitOfWork unitOfWork, IMapper mapper, ITaskNotifier notifier,
        IReadOnlyList<TaskActivity> entries, IEnumerable<int> alsoNotify)
    {
        try
        {
            var actors = new Dictionary<int, UserDto?>();

            foreach (var group in entries.GroupBy(e => e.TaskItemId))
            {
                // A new comment already raises its own "commented on" pop-up
                // (to everyone on the project, including whoever it's assigned
                // to), so its history entries would only repeat it.
                var commentCreated = group.Any(e => e.Type == TaskActivityType.CommentAdded);
                var toSend = group
                    .Where(e => !commentCreated
                        || (e.Type != TaskActivityType.CommentAdded && e.Type != TaskActivityType.CommentAssigned))
                    .ToList();
                if (toSend.Count == 0) continue;

                // Assignees as they are after the change.
                var task = await unitOfWork.Repository<TaskItem>()
                    .GetEntityWithSpec(new TaskByIdWithAssigneesSpecification(group.Key));
                if (task is null) continue;

                var everyone = new HashSet<int>(alsoNotify);
                foreach (var u in task.Assignees) everyone.Add(u.Id);

                foreach (var entry in toSend)
                {
                    var recipients = everyone.Where(id => id != entry.UserId).ToList();
                    if (recipients.Count == 0) continue;

                    if (!actors.TryGetValue(entry.UserId, out var actor))
                    {
                        var user = await unitOfWork.Repository<User>().GetByIdAsync(entry.UserId);
                        actor = user is null ? null : mapper.Map<UserDto>(user);
                        actors[entry.UserId] = actor;
                    }

                    var activity = mapper.Map<TaskActivityDto>(entry);
                    activity.User = actor;
                    await notifier.TaskChangedAsync(recipients, new TaskChangeDto
                    {
                        TaskId = task.Id,
                        TaskTitle = task.Title,
                        ProjectId = task.ProjectId,
                        Activity = activity,
                    });
                }
            }
        }
        catch
        {
            // Best effort: the change is saved and shows in the task's history.
        }
    }
}
