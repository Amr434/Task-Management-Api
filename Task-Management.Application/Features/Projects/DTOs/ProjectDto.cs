namespace Task_Management.Application.Features.Projects.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SpaceId { get; set; }
    
    // We could return Lists inside the project, but we'll leave it out for simplicity unless needed
    public ICollection<ProjectMemberDto> Members { get; set; } = new List<ProjectMemberDto>();

    // Custom board statuses (built-in ToDo/InProgress/Complete are implicit).
    public List<ProjectStatusDto> Statuses { get; set; } = new();
}

public class ProjectStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#7b68ee";
}

public class ProjectMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    // Profile picture, relative to the API base URL (null = no picture).
    public string? AvatarUrl { get; set; }
}
