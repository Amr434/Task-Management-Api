namespace Task_Management.Application.Features.Spaces.DTOs;

public class SpaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public ICollection<SpaceMemberDto> Members { get; set; } = new List<SpaceMemberDto>();
}

public class SpaceMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
}
