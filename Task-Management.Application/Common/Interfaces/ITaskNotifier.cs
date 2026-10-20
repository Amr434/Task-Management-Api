using Task_Management.Application.Features.Tasks.DTOs;

namespace Task_Management.Application.Common.Interfaces;

// Implemented in the Api layer with SignalR: tells users' open web/mobile
// sessions that a task they're on was changed (assigned, status, due date...),
// so they can show a notification. No-ops for users who aren't connected.
public interface ITaskNotifier
{
    Task TaskChangedAsync(IEnumerable<int> recipientUserIds, TaskChangeDto change);
}
