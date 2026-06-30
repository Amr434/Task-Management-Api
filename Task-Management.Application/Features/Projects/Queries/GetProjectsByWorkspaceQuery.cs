using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Projects;

using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Projects.Queries;

public class GetProjectsByWorkspaceQuery : IRequest<Result<IEnumerable<ProjectDto>>>
{
    public int WorkspaceId { get; set; }

    public GetProjectsByWorkspaceQuery(int workspaceId)
    {
        WorkspaceId = workspaceId;
    }
}

public class GetProjectsByWorkspaceQueryHandler : IRequestHandler<GetProjectsByWorkspaceQuery, Result<IEnumerable<ProjectDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProjectsByWorkspaceQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ProjectDto>>> Handle(GetProjectsByWorkspaceQuery request, CancellationToken cancellationToken)
    {
        // Use the specification to filter by WorkspaceId and eager-load Lists
        var spec = new ProjectsWithListsSpecification(request.WorkspaceId);
        
        var projects = await _unitOfWork.Repository<Project>().ListAsync(spec);
        
        var dtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
        return Result.Success(dtos);
    }
}
