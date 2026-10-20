namespace Task_Management.Application.Features.Users.DTOs;

// A user as seen on the user-management screen.
public class ManagedUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; } // UserRole: Member=0, Admin=1, SuperAdmin=2
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public string? AvatarUrl { get; set; }
    // Whether the current user may manage this user (activate/deactivate,
    // reset password). The frontend uses it to show or hide the actions.
    public bool CanManage { get; set; }
}

public class UpdateUserRoleDto
{
    public int Role { get; set; } // 0 = Member, 1 = Admin
}

public class SetUserStatusDto
{
    public bool IsActive { get; set; }
}

public class ResetUserPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}
