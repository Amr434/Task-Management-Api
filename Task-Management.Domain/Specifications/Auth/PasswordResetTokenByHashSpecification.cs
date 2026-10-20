using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Auth;

// The reset request a link belongs to (looked up by the hash of its token),
// with the user whose password it resets.
public class PasswordResetTokenByHashSpecification : BaseSpecification<PasswordResetToken>
{
    public PasswordResetTokenByHashSpecification(string tokenHash)
        : base(t => t.TokenHash == tokenHash)
    {
        AddInclude(t => t.User);
    }
}
