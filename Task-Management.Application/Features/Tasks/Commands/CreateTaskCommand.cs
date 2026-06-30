using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tasks.Commands;

public class CreateTaskCommand : IRequest<Result<TaskItemDto>>
{
    public CreateTaskDto TaskDto { get; set; }

    public CreateTaskCommand(CreateTaskDto taskDto)
    {
        TaskDto = taskDto;
    }
}

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(v => v.TaskDto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
            
        RuleFor(v => v.TaskDto.ListId)
            .GreaterThan(0).WithMessage("ListId must be valid.");
    }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TaskItemDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = _mapper.Map<TaskItem>(request.TaskDto);

        _unitOfWork.Repository<TaskItem>().Add(task);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<TaskItemDto>(task);
        return Result.Success(dto);
    }
}
