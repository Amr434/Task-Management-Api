using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Commands;

public class RemoveTagFromTaskCommand : IRequest<Result<bool>>
{
    public int TaskId { get; set; }
    public int TagId { get; set; }
    public int ActorId { get; set; }

    public RemoveTagFromTaskCommand(int taskId, int tagId, int actorId)
    {
        TaskId = taskId;
        TagId = tagId;
        ActorId = actorId;
    }
}

public class RemoveTagFromTaskCommandHandler : IRequestHandler<RemoveTagFromTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveTagFromTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RemoveTagFromTaskCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the task and eager-load its existing tags
        var spec = new TaskByIdWithTagsSpecification(request.TaskId);
        var task = await _unitOfWork.Repository<TaskItem>().GetEntityWithSpec(spec);

        if (task == null)
        {
            return Result.Failure<bool>(new Error("Task.NotFound", $"Task with Id {request.TaskId} was not found."));
        }

        // 2. Find the assigned tag on the task
        var tag = task.Tags.FirstOrDefault(t => t.Id == request.TagId);

        if (tag == null)
        {
            return Result.Failure<bool>(new Error("Task.TagNotAssigned", "This tag is not assigned to the task."));
        }

        // 3. Remove the relationship and save — EF Core tracks the join-table deletion
        TaskHistory.Record(task, request.ActorId, TaskActivityType.TagRemoved, oldValue: tag.Name);
        task.Tags.Remove(tag);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
