using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Auth;

// A user's reset links that haven't been used or switched off yet
// (they may still have expired; callers check ExpiresAtUtc where it matters).
public class OpenPasswordResetTokensForUserSpecification : BaseSpecification<PasswordResetToken>
{
    public OpenPasswordResetTokensForUserSpecification(int userId)
        : base(t => t.UserId == userId && t.UsedAtUtc == null)
    {
    }
}
