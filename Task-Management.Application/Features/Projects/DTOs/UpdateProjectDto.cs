namespace Task_Management.Application.Features.Projects.DTOs;

public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SpaceId { get; set; }
}
