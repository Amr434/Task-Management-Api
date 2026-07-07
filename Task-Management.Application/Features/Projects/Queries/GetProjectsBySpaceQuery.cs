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

    public GetProjectsBySpaceQuery(int spaceId)
    {
        SpaceId = spaceId;
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
        var spec = new ProjectsBySpaceSpecification(request.SpaceId);
        
        var projects = await _unitOfWork.Repository<Project>().ListAsync(spec);
        
        var dtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);
        return Result.Success(dtos);
    }
}
