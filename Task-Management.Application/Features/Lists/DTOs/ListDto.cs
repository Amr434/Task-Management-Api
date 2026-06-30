namespace Task_Management.Application.Features.Lists.DTOs;

public class ListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int ProjectId { get; set; }
}
