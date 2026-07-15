using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Dashboards;
using Task_Management.Domain.Specifications.Spaces;

namespace Task_Management.Application.Features.Dashboards.Commands;

public class RenameDashboardCommand : IRequest<Result<bool>>
{
    public int DashboardId { get; set; }
    public string NewName { get; set; }
    public int UserId { get; set; }

    public RenameDashboardCommand(int dashboardId, string newName, int userId)
    {
        DashboardId = dashboardId;
        NewName = newName;
        UserId = userId;
    }
}

public class RenameDashboardCommandHandler : IRequestHandler<RenameDashboardCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RenameDashboardCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RenameDashboardCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewName))
        {
            return Result.Failure<bool>(new Error("Dashboard.InvalidName", "Dashboard name cannot be empty."));
        }

        var dashboard = await _unitOfWork.Repository<Dashboard>()
            .GetEntityWithSpec(new DashboardByIdSpecification(request.DashboardId));

        if (dashboard is null)
        {
            return Result.Failure<bool>(new Error("Dashboard.NotFound", "Dashboard not found."));
        }

        var hasAccess = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new AccessibleSpaceSpecification(dashboard.SpaceId, request.UserId));

        if (hasAccess is null)
        {
            return Result.Failure<bool>(new Error("Dashboard.Forbidden", "You don't have access to this dashboard."));
        }

        dashboard.Name = request.NewName;
        
        _unitOfWork.Repository<Dashboard>().Update(dashboard);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
