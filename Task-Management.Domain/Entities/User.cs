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
    // Profile picture, relative to the file storage folder (null = none).
    // The file name is unique per upload, so it doubles as a cache-buster.
    public string? AvatarPath { get; set; }

    // Email settings. Every email kind is on by default; MutedEmailCategories
    // holds one bit per EmailCategory the user turned off.
    public EmailDeliveryMode EmailMode { get; set; } = EmailDeliveryMode.Instant;
    public int MutedEmailCategories { get; set; }
    public SummaryFrequency EmailSummary { get; set; } = SummaryFrequency.Off;
    // When the last digest / summary email went out, so each goes once per period.
    public DateTime? LastDigestSentAtUtc { get; set; }
    public DateTime? LastSummarySentAtUtc { get; set; }

    public bool WantsEmail(EmailCategory category) => (MutedEmailCategories & (1 << (int)category)) == 0;

    // Navigation properties
    public ICollection<Space> Spaces { get; set; } = new List<Space>();
    public ICollection<Project> SharedProjects { get; set; } = new List<Project>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
