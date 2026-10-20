using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Projects;

using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Projects.Queries;

public class GetProjectsBySpaceQuery : IRequest<Result<IEnumerable<ProjectDto>>>
{
    public int SpaceId { get; set; }
    public int UserId { get; set; }

    public GetProjectsBySpaceQuery(int spaceId, int userId)
    {
        SpaceId = spaceId;
        UserId = userId;
    }
}

public class GetProjectsBySpaceQueryHandler : IRequestHandler<GetProjectsBySpaceQuery, Result<IEnumerable<ProjectDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProjectsBySpaceQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ProjectDto>>> Handle(GetProjectsBySpaceQuery request, CancellationToken cancellationToken)
    {
        // Space-level access (owner/member) sees every project; otherwise the
        // user only sees projects that were shared with them directly.
        var spaceAccess = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new Task_Management.Domain.Specifications.Spaces.AccessibleSpaceSpecification(request.SpaceId, request.UserId));

        var projects = spaceAccess is not null
            ? await _unitOfWork.Repository<Project>().ListAsync(new ProjectsBySpaceSpecification(request.SpaceId))
            : await _unitOfWork.Repository<Project>().ListAsync(new SharedProjectsInSpaceSpecification(request.SpaceId, request.UserId));

        var dtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
        return Result.Success(dtos);
    }
}
