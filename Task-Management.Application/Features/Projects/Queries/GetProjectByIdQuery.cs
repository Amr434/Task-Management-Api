using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Projects;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Projects.Queries;

public class GetProjectByIdQuery : IRequest<Result<ProjectDto>>
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }

    public GetProjectByIdQuery(int projectId, int userId)
    {
        ProjectId = projectId;
        UserId = userId;
    }
}

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, Result<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>()
            .GetEntityWithSpec(new AccessibleProjectSpecification(request.ProjectId, request.UserId));
        if (project is null)
        {
            return Result.Failure<ProjectDto>(new Error("Project.NotFound", "Project not found or you don't have access to it."));
        }

        return Result.Success(_mapper.Map<ProjectDto>(project));
    }
}
