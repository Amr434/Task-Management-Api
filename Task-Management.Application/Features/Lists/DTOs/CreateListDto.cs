namespace Task_Management.Application.Features.Lists.DTOs;

public class CreateListDto
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int ProjectId { get; set; }
}
