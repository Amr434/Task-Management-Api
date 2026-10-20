using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Commands;

public class RemoveUserFromTaskCommand : IRequest<Result<bool>>, ITaskChangeRecipients
{
    public int TaskId { get; set; }
    public int UserId { get; set; }

    // Who made the change, for the task history (UserId is the assignee).
    public int ActorId { get; set; }

    public RemoveUserFromTaskCommand(int taskId, int userId, int actorId)
    {
        TaskId = taskId;
        UserId = userId;
        ActorId = actorId;
    }

    // The removed person hears about it, though no longer an assignee.
    public IEnumerable<int> AlsoNotify => new[] { UserId };
}

public class RemoveUserFromTaskCommandHandler : IRequestHandler<RemoveUserFromTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserFromTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RemoveUserFromTaskCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the task and eager-load its existing assignees
        var spec = new TaskByIdWithAssigneesSpecification(request.TaskId);
        var task = await _unitOfWork.Repository<TaskItem>().GetEntityWithSpec(spec);

        if (task == null)
        {
            return Result.Failure<bool>(new Error("Task.NotFound", $"Task with Id {request.TaskId} was not found."));
        }

        // 2. Find the assignee on the task
        var assignee = task.Assignees.FirstOrDefault(u => u.Id == request.UserId);

        if (assignee == null)
        {
            return Result.Failure<bool>(new Error("Task.UserNotAssigned", "This user is not assigned to the task."));
        }

        // 3. Remove the relationship and save
        TaskHistory.Record(task, request.ActorId, TaskActivityType.AssigneeRemoved, oldValue: TaskHistory.PersonName(assignee));
        task.Assignees.Remove(assignee);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
