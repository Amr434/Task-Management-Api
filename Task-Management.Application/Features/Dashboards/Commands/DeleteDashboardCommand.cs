using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Dashboards;
using Task_Management.Domain.Specifications.Spaces;

namespace Task_Management.Application.Features.Dashboards.Commands;

public class DeleteDashboardCommand : IRequest<Result<bool>>
{
    public int DashboardId { get; set; }
    public int UserId { get; set; }

    public DeleteDashboardCommand(int dashboardId, int userId)
    {
        DashboardId = dashboardId;
        UserId = userId;
    }
}

public class DeleteDashboardCommandHandler : IRequestHandler<DeleteDashboardCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDashboardCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteDashboardCommand request, CancellationToken cancellationToken)
    {
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

        _unitOfWork.Repository<Dashboard>().Delete(dashboard);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
