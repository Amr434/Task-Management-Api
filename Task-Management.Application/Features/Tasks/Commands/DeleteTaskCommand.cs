using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tasks.Commands;

public class DeleteTaskCommand : IRequest<Result<bool>>
{
    public int TaskId { get; set; }

    public DeleteTaskCommand(int taskId)
    {
        TaskId = taskId;
    }
}

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskId);

        if (task == null)
        {
            return Result.Failure<bool>(new Error("Task.NotFound", $"Task with Id {request.TaskId} was not found."));
        }

        _unitOfWork.Repository<TaskItem>().Delete(task);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
