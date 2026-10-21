using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Commands;

public class DeleteTaskCommand : IRequest<Result<bool>>
{
    public int TaskId { get; set; }

    // Who is deleting; only the task's creator may.
    public int ActorId { get; set; }

    public DeleteTaskCommand(int taskId, int actorId)
    {
        TaskId = taskId;
        ActorId = actorId;
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
        var repo = _unitOfWork.Repository<TaskItem>();
        var task = await repo.GetByIdAsync(request.TaskId);

        if (task == null)
        {
            return Result.Failure<bool>(new Error("Task.NotFound", $"Task with Id {request.TaskId} was not found."));
        }

        if (task.CreatedById != request.ActorId)
        {
            return Result.Failure<bool>(new Error("Task.Forbidden", "Only the person who created this task can delete it."));
        }

        // Subtasks go with their parent, whoever created them. Collect the whole
        // subtree level by level; EF deletes children before parents on save.
        var toDelete = new List<TaskItem> { task };
        var level = new List<int> { task.Id };
        while (level.Count > 0)
        {
            var children = await repo.ListAsync(new TasksByParentIdsSpecification(level));
            toDelete.AddRange(children);
            level = children.Select(c => c.Id).ToList();
        }

        foreach (var t in toDelete)
        {
            repo.Delete(t);
        }
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
