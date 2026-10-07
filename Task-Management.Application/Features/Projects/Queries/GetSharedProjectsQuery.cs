using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Projects;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Projects.Queries;

// "Shared with me": projects shared directly with the user whose parent
// space they can't see (space-level shares surface as whole spaces instead).
public class GetSharedProjectsQuery : IRequest<Result<IEnumerable<ProjectDto>>>
{
    public int UserId { get; set; }

    public GetSharedProjectsQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetSharedProjectsQueryHandler : IRequestHandler<GetSharedProjectsQuery, Result<IEnumerable<ProjectDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSharedProjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ProjectDto>>> Handle(GetSharedProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Repository<Project>()
            .ListAsync(new SharedProjectsForUserSpecification(request.UserId));

        return Result.Success(_mapper.Map<IEnumerable<ProjectDto>>(projects));
    }
}
