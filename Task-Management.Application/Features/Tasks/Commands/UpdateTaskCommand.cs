using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tasks.Commands;

public class UpdateTaskCommand : IRequest<Result<TaskItemDto>>
{
    public int TaskId { get; set; }
    public UpdateTaskDto TaskDto { get; set; }

    public UpdateTaskCommand(int taskId, UpdateTaskDto taskDto)
    {
        TaskId = taskId;
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

        task.Title = request.TaskDto.Title;
        task.Description = request.TaskDto.Description;
        task.DueDate = request.TaskDto.DueDate;
        task.Priority = request.TaskDto.Priority;
        task.Order = request.TaskDto.Order;
        task.ProjectId = request.TaskDto.ProjectId;
        task.Status = request.TaskDto.Status;
        task.ParentTaskId = request.TaskDto.ParentTaskId;

        _unitOfWork.Repository<TaskItem>().Update(task);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<TaskItemDto>(task);
        return Result.Success(dto);
    }
}
