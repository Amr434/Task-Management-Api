using Task_Management.Domain.Entities;

namespace Task_Management.Domain.Specifications.Auth;

// Refresh tokens of a user that haven't been revoked yet. Used to sign a user
// out everywhere when they're deactivated or their password is reset.
public class ActiveRefreshTokensForUserSpecification : BaseSpecification<RefreshToken>
{
    public ActiveRefreshTokensForUserSpecification(int userId)
        : base(rt => rt.UserId == userId && rt.RevokedAtUtc == null)
    {
    }
}
