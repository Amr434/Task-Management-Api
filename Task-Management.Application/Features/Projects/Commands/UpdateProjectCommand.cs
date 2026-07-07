using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Projects.Commands;

public class UpdateProjectCommand : IRequest<Result<ProjectDto>>
{
    public int Id { get; set; }
    public UpdateProjectDto ProjectDto { get; set; }

    public UpdateProjectCommand(int id, UpdateProjectDto projectDto)
    {
        Id = id;
        ProjectDto = projectDto;
    }
}

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(v => v.ProjectDto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(v => v.ProjectDto.SpaceId)
            .GreaterThan(0).WithMessage("SpaceId must be valid.");
    }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, Result<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>().GetByIdAsync(request.Id);

        if (project == null)
        {
            return Result.Failure<ProjectDto>(new Error("Project.NotFound", "Project not found."));
        }

        project.Name = request.ProjectDto.Name;
        project.Description = request.ProjectDto.Description;
        project.SpaceId = request.ProjectDto.SpaceId;

        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<ProjectDto>(project);
        return Result.Success(dto);
    }
}
