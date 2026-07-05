using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Projects.Commands;

public class CreateProjectCommand : IRequest<Result<ProjectDto>>
{
    public CreateProjectDto ProjectDto { get; set; }

    public CreateProjectCommand(CreateProjectDto projectDto)
    {
        ProjectDto = projectDto;
    }
}

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(v => v.ProjectDto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
            
        RuleFor(v => v.ProjectDto.SpaceId)
            .GreaterThan(0).WithMessage("SpaceId must be valid.");
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = _mapper.Map<Project>(request.ProjectDto);

        _unitOfWork.Repository<Project>().Add(project);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<ProjectDto>(project);
        return Result.Success(dto);
    }
}
