using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Commands;

public class AssignTagToTaskCommand : IRequest<Result<bool>>
{
    public int TaskId { get; set; }
    public int TagId { get; set; }

    public AssignTagToTaskCommand(int taskId, int tagId)
    {
        TaskId = taskId;
        TagId = tagId;
    }
}

public class AssignTagToTaskCommandHandler : IRequestHandler<AssignTagToTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignTagToTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(AssignTagToTaskCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the task and eager-load its existing tags
        var spec = new TaskByIdWithTagsSpecification(request.TaskId);
        var task = await _unitOfWork.Repository<TaskItem>().GetEntityWithSpec(spec);

        if (task == null)
        {
            return Result.Failure<bool>(new Error("Task.NotFound", $"Task with Id {request.TaskId} was not found."));
        }

        // 2. Fetch the tag we want to assign
        var tag = await _unitOfWork.Repository<Tag>().GetByIdAsync(request.TagId);

        if (tag == null)
        {
            return Result.Failure<bool>(new Error("Tag.NotFound", $"Tag with Id {request.TagId} was not found."));
        }

        // 3. Ensure the tag isn't already assigned to avoid duplicates
        if (task.Tags.Any(t => t.Id == request.TagId))
        {
            return Result.Failure<bool>(new Error("Task.TagAlreadyAssigned", "This tag is already assigned to the task."));
        }

        // 4. Assign the tag and save
        task.Tags.Add(tag);
        
        // EF Core will automatically track that a new relationship was added to the join table
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
