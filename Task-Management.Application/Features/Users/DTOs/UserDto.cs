namespace Task_Management.Application.Features.Users.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; } // UserRole: Member=0, Admin=1, SuperAdmin=2
    // Relative to the API base URL, e.g. "Users/5/avatar?v=..." (null = no picture).
    public string? AvatarUrl { get; set; }
}
