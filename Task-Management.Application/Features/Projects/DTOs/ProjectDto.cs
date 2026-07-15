namespace Task_Management.Application.Features.Projects.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SpaceId { get; set; }
    
    // We could return Lists inside the project, but we'll leave it out for simplicity unless needed
    public ICollection<ProjectMemberDto> Members { get; set; } = new List<ProjectMemberDto>();
}

public class ProjectMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
}
