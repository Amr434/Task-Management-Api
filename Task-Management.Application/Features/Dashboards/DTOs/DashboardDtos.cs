namespace Task_Management.Application.Features.Dashboards.DTOs;

public class DashboardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerInitials { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastViewedAt { get; set; }
    public bool IsSpaceShared { get; set; }
    public ICollection<DashboardMemberDto> SharingMembers { get; set; } = new List<DashboardMemberDto>();
}

public class DashboardMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
}

public class CreateDashboardDto
{
    public int SpaceId { get; set; }
}

public class AssigneeCountDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public int? UserId { get; set; }
}

public class WorkloadByStatusDto
{
    public int Unassigned { get; set; }
    public int Assigned { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
}

public class DashboardSummaryDto
{
    public DashboardDto Dashboard { get; set; } = new();
    public string SpaceName { get; set; } = string.Empty;
    public string? SpaceDescription { get; set; }
    public string? SpaceColor { get; set; }
    public string? SpaceIcon { get; set; }
    public bool IsSpaceShared { get; set; }
    public WorkloadByStatusDto? WorkloadByStatus { get; set; }
    public ICollection<AssigneeCountDto> TasksByAssignee { get; set; } = new List<AssigneeCountDto>();
    public ICollection<AssigneeCountDto> OpenTasksByAssignee { get; set; } = new List<AssigneeCountDto>();
    public ICollection<AssigneeCountDto> CompletedThisWeekByAssignee { get; set; } = new List<AssigneeCountDto>();
}

public class RenameDashboardDto
{
    public string Name { get; set; } = string.Empty;
}
