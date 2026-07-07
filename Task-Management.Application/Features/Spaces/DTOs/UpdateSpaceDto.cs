namespace Task_Management.Application.Features.Spaces.DTOs;

public class UpdateSpaceDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}
