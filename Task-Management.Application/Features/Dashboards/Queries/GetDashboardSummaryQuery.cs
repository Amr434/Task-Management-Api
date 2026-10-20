using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Dashboards.Commands;
using Task_Management.Application.Features.Dashboards.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Dashboards;
using Task_Management.Domain.Specifications.Spaces;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Dashboards.Queries;

public class GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryDto>>
{
    public int DashboardId { get; set; }
    public int UserId { get; set; }

    public GetDashboardSummaryQuery(int dashboardId, int userId)
    {
        DashboardId = dashboardId;
        UserId = userId;
    }
}

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDashboardSummaryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var dashboard = await _unitOfWork.Repository<Dashboard>()
            .GetEntityWithSpec(new DashboardByIdSpecification(request.DashboardId));

        if (dashboard is null)
        {
            return Result.Failure<DashboardSummaryDto>(new Error("Dashboard.NotFound", "Dashboard not found."));
        }

        var hasAccess = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new AccessibleSpaceSpecification(dashboard.SpaceId, request.UserId));

        if (hasAccess is null)
        {
            return Result.Failure<DashboardSummaryDto>(new Error("Dashboard.Forbidden", "You don't have access to this dashboard."));
        }

        dashboard.LastViewedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Dashboard>().Update(dashboard);
        await _unitOfWork.CompleteAsync();

        var spaceWithMembers = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new SpaceWithMembersSpecification(dashboard.SpaceId));

        var isShared = spaceWithMembers?.Members.Count > 0;

        var tasks = await _unitOfWork.Repository<TaskItem>()
            .ListAsync(new TasksInSpaceSpecification(dashboard.SpaceId));

        var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);

        if (spaceWithMembers is not null)
            dashboard.Space = spaceWithMembers;

        var summary = new DashboardSummaryDto
        {
            Dashboard = _mapper.Map<DashboardDto>(dashboard),
            SpaceName = spaceWithMembers?.Name ?? string.Empty,
            SpaceDescription = spaceWithMembers?.Description,
            SpaceColor = spaceWithMembers?.Color,
            SpaceIcon = spaceWithMembers?.Icon,
            IsSpaceShared = isShared,
            WorkloadByStatus = isShared ? BuildWorkload(tasks) : null,
            TasksByAssignee = GroupByAssignee(tasks),
            OpenTasksByAssignee = GroupByAssignee(tasks.Where(t => t.Status != TaskStatusLevel.Complete)),
            CompletedThisWeekByAssignee = GroupByAssignee(
                tasks.Where(t =>
                    t.Status == TaskStatusLevel.Complete &&
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date >= startOfWeek &&
                    t.DueDate.Value.Date <= DateTime.UtcNow.Date)),
        };

        return Result.Success(summary);
    }

    private static WorkloadByStatusDto BuildWorkload(IReadOnlyList<TaskItem> tasks)
    {
        return new WorkloadByStatusDto
        {
            Unassigned = tasks.Count(t => t.Assignees.Count == 0),
            Assigned = tasks.Count(t => t.Assignees.Count > 0 && t.Status == TaskStatusLevel.ToDo),
            InProgress = tasks.Count(t => t.Status == TaskStatusLevel.InProgress),
            Completed = tasks.Count(t => t.Status == TaskStatusLevel.Complete),
        };
    }

    private static List<AssigneeCountDto> GroupByAssignee(IEnumerable<TaskItem> tasks)
    {
        var counts = new Dictionary<string, (int Count, int? UserId)>();

        foreach (var task in tasks)
        {
            if (task.Assignees.Count == 0)
            {
                AddCount(counts, "Unassigned", null);
            }
            else
            {
                foreach (var assignee in task.Assignees)
                {
                    var label = $"{assignee.FirstName} {assignee.LastName}".Trim();
                    AddCount(counts, label, assignee.Id);
                }
            }
        }

        return counts
            .OrderByDescending(kv => kv.Value.Count)
            .Select(kv => new AssigneeCountDto
            {
                Label = kv.Key,
                Count = kv.Value.Count,
                UserId = kv.Value.UserId,
            })
            .ToList();
    }

    private static void AddCount(Dictionary<string, (int Count, int? UserId)> counts, string label, int? userId)
    {
        if (counts.TryGetValue(label, out var existing))
            counts[label] = (existing.Count + 1, existing.UserId ?? userId);
        else
            counts[label] = (1, userId);
    }
}
