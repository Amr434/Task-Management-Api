namespace Task_Management.Domain.Entities;

// One "forgot my password" request. The link emailed to the user carries a
// random token; only its SHA-256 hash is stored here, so someone who can read
// the database still can't use the links. A token works once and expires.
public class PasswordResetToken : BaseEntity
{
    public string TokenHash { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    // Set when the link stops working: the password was changed (with this
    // link or any other way) or the account was deactivated.
    public DateTime? UsedAtUtc { get; set; }
}
