using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Commands;

public class AssignUserToTaskCommand : IRequest<Result<bool>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }

    public AssignUserToTaskCommand(int taskId, int userId)
    {
        TaskId = taskId;
        UserId = userId;
    }
}

public class AssignUserToTaskCommandHandler : IRequestHandler<AssignUserToTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignUserToTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(AssignUserToTaskCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the task and eager-load its existing assignees
        var spec = new TaskByIdWithAssigneesSpecification(request.TaskId);
        var task = await _unitOfWork.Repository<TaskItem>().GetEntityWithSpec(spec);

        if (task == null)
        {
            return Result.Failure<bool>(new Error("Task.NotFound", $"Task with Id {request.TaskId} was not found."));
        }

        // 2. Fetch the user we want to assign
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);

        if (user == null)
        {
            return Result.Failure<bool>(new Error("User.NotFound", $"User with Id {request.UserId} was not found."));
        }

        // 3. Ensure the user isn't already assigned to avoid duplicates
        if (task.Assignees.Any(u => u.Id == request.UserId))
        {
            return Result.Failure<bool>(new Error("Task.UserAlreadyAssigned", "This user is already assigned to the task."));
        }

        // 4. Assign the user and save
        task.Assignees.Add(user);

        // EF Core will automatically track that a new relationship was added to the join table
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
