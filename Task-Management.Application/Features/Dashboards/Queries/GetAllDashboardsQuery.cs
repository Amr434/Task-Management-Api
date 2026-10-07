using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Dashboards.Commands;
using Task_Management.Application.Features.Dashboards.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Dashboards;
using Task_Management.Domain.Specifications.Spaces;

namespace Task_Management.Application.Features.Dashboards.Queries;

public class GetAllDashboardsQuery : IRequest<Result<IEnumerable<DashboardDto>>>
{
    public int UserId { get; set; }

    public GetAllDashboardsQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetAllDashboardsQueryHandler : IRequestHandler<GetAllDashboardsQuery, Result<IEnumerable<DashboardDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllDashboardsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<DashboardDto>>> Handle(GetAllDashboardsQuery request, CancellationToken cancellationToken)
    {
        var spec = new DashboardsForUserSpecification(request.UserId);
        var dashboards = await _unitOfWork.Repository<Dashboard>().ListAsync(spec);

        var dtos = new List<DashboardDto>();
        foreach (var dashboard in dashboards)
        {
            var spaceWithMembers = await _unitOfWork.Repository<Space>()
                .GetEntityWithSpec(new SpaceWithMembersSpecification(dashboard.SpaceId));
            if (spaceWithMembers is not null)
                dashboard.Space = spaceWithMembers;

            dtos.Add(_mapper.Map<DashboardDto>(dashboard));
        }

        return Result.Success<IEnumerable<DashboardDto>>(dtos);
    }
}
