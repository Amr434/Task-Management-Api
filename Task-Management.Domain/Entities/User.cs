using Task_Management.Domain.Enums;

namespace Task_Management.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Identity or auth provider ID
    public string ExternalId { get; set; } = string.Empty;

    // Authentication (offline/self-hosted: hash stored locally, no external provider)
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public bool IsActive { get; set; } = true;
    // Admin-created accounts get a temporary password; force a change on first login.
    public bool MustChangePassword { get; set; }

    // Navigation properties
    public ICollection<Space> Spaces { get; set; } = new List<Space>();
    public ICollection<Project> SharedProjects { get; set; } = new List<Project>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
