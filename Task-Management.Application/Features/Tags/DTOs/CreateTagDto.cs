namespace Task_Management.Application.Features.Tags.DTOs;

public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#000000";
}
