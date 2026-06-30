namespace Task_Management.Application.Features.Projects.DTOs;

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int WorkspaceId { get; set; }
}
