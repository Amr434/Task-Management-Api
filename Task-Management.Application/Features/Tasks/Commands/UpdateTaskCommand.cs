using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tasks.Commands;

public class UpdateTaskCommand : IRequest<Result<TaskItemDto>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public UpdateTaskDto TaskDto { get; set; }

    public UpdateTaskCommand(int taskId, int userId, UpdateTaskDto taskDto)
    {
        TaskId = taskId;
        UserId = userId;
        TaskDto = taskDto;
    }
}

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(v => v.TaskId)
            .GreaterThan(0).WithMessage("TaskId must be valid.");

        RuleFor(v => v.TaskDto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
            
        RuleFor(v => v.TaskDto.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be valid.");
    }
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<TaskItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TaskItemDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskId);

        if (task == null)
        {
            return Result.Failure<TaskItemDto>(new Error("Task.NotFound", $"Task with Id {request.TaskId} was not found."));
        }

        var dto = request.TaskDto;

        // Log each field that actually changed. Order and parent are left out:
        // reordering on a board is noise, not history.
        if (task.Title != dto.Title)
            TaskHistory.Record(task, request.UserId, TaskActivityType.TitleChanged, task.Title, dto.Title);
        if ((task.Description ?? "") != (dto.Description ?? ""))
            TaskHistory.Record(task, request.UserId, TaskActivityType.DescriptionChanged);
        if (task.Status != dto.Status)
            TaskHistory.Record(task, request.UserId, TaskActivityType.StatusChanged,
                task.Status.ToString(), dto.Status.ToString());
        if (task.Priority != dto.Priority)
            TaskHistory.Record(task, request.UserId, TaskActivityType.PriorityChanged,
                task.Priority.ToString(), dto.Priority.ToString());
        if (TaskHistory.DateText(task.DueDate) != TaskHistory.DateText(dto.DueDate))
            TaskHistory.Record(task, request.UserId, TaskActivityType.DueDateChanged,
                TaskHistory.DateText(task.DueDate), TaskHistory.DateText(dto.DueDate));
        if (task.ProjectId != dto.ProjectId)
        {
            var from = await _unitOfWork.Repository<Project>().GetByIdAsync(task.ProjectId);
            var to = await _unitOfWork.Repository<Project>().GetByIdAsync(dto.ProjectId);
            TaskHistory.Record(task, request.UserId, TaskActivityType.MovedToProject, from?.Name, to?.Name);
        }

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.DueDate = dto.DueDate;
        task.Priority = dto.Priority;
        task.Order = dto.Order;
        task.ProjectId = dto.ProjectId;
        task.Status = dto.Status;
        task.ParentTaskId = dto.ParentTaskId;

        _unitOfWork.Repository<TaskItem>().Update(task);
        await _unitOfWork.CompleteAsync();

        return Result.Success(_mapper.Map<TaskItemDto>(task));
    }
}
