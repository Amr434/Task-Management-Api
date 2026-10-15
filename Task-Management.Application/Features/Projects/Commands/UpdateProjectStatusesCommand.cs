using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications;
using Task_Management.Domain.Specifications.Projects;

namespace Task_Management.Application.Features.Projects.Commands;

// Replaces a project's custom board statuses. Statuses sent without an id
// (or with id < 3) get a fresh one; tasks in a status that was removed go
// back to To Do.
public class UpdateProjectStatusesCommand : IRequest<Result<ProjectDto>>
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public List<ProjectStatusDto> Statuses { get; set; }

    public UpdateProjectStatusesCommand(int projectId, int userId, List<ProjectStatusDto> statuses)
    {
        ProjectId = projectId;
        UserId = userId;
        Statuses = statuses;
    }
}

public class UpdateProjectStatusesCommandValidator : AbstractValidator<UpdateProjectStatusesCommand>
{
    public UpdateProjectStatusesCommandValidator()
    {
        RuleFor(v => v.Statuses).NotNull();
        RuleFor(v => v.Statuses.Count).LessThanOrEqualTo(20).WithMessage("A project can have at most 20 custom statuses.");
        RuleForEach(v => v.Statuses).ChildRules(s =>
        {
            s.RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Status name is required.")
                .MaximumLength(40).WithMessage("Status name must not exceed 40 characters.");
            s.RuleFor(x => x.Color)
                .Matches("^#[0-9a-fA-F]{6}$").WithMessage("Color must be a hex value like #7b68ee.");
        });
    }
}

public class UpdateProjectStatusesCommandHandler : IRequestHandler<UpdateProjectStatusesCommand, Result<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProjectStatusesCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDto>> Handle(UpdateProjectStatusesCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>()
            .GetEntityWithSpec(new AccessibleProjectSpecification(request.ProjectId, request.UserId));
        if (project is null)
        {
            return Result.Failure<ProjectDto>(new Error("Project.NotFound", "Project not found or you don't have access to it."));
        }

        var existing = ProjectStatuses.Parse(project.CustomStatuses);
        var nextId = Math.Max(ProjectStatuses.FirstCustomId, existing.Select(s => s.Id + 1).DefaultIfEmpty(0).Max());

        var statuses = new List<ProjectStatusDto>();
        foreach (var s in request.Statuses)
        {
            var keep = s.Id >= ProjectStatuses.FirstCustomId && existing.Any(e => e.Id == s.Id) && statuses.All(x => x.Id != s.Id);
            statuses.Add(new ProjectStatusDto
            {
                Id = keep ? s.Id : nextId++,
                Name = s.Name.Trim(),
                Color = s.Color,
            });
        }

        var removed = existing.Where(e => statuses.All(s => s.Id != e.Id)).Select(e => (TaskStatusLevel)e.Id).ToList();
        if (removed.Count > 0)
        {
            var orphaned = await _unitOfWork.Repository<TaskItem>().ListAsync(
                new BaseSpecification<TaskItem>(t => t.ProjectId == project.Id && removed.Contains(t.Status)));
            foreach (var task in orphaned)
            {
                task.Status = TaskStatusLevel.ToDo;
                _unitOfWork.Repository<TaskItem>().Update(task);
            }
        }

        project.CustomStatuses = ProjectStatuses.Serialize(statuses);
        _unitOfWork.Repository<Project>().Update(project);
        await _unitOfWork.CompleteAsync();

        return Result.Success(_mapper.Map<ProjectDto>(project));
    }
}
