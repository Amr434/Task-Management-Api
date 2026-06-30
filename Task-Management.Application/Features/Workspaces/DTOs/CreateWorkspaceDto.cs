namespace Task_Management.Application.Features.Workspaces.DTOs;

public class CreateWorkspaceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
