using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Commands;

public class RemoveUserFromTaskCommand : IRequest<Result<bool>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }

    public RemoveUserFromTaskCommand(int taskId, int userId)
    {
        TaskId = taskId;
        UserId = userId;
    }
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
        task.Assignees.Remove(assignee);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
