using System.Security.Cryptography;
using System.Text;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Auth;

namespace Task_Management.Application.Features.Auth;

// Rules for "forgot my password" links, in one place.
public static class PasswordResetTokens
{
    // How long an emailed link works.
    public const int LifetimeMinutes = 60;

    // A second request within this time doesn't send another email, and no
    // more than MaxOpenLinks unexpired links exist per person at once
    // (stops someone flooding a person's inbox).
    public const int ResendCooldownSeconds = 60;
    public const int MaxOpenLinks = 3;

    // The random value that goes in the link: 64 hex characters, safe in a URL.
    public static string NewToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    // What we store instead of the token itself.
    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    // Switches off every reset link the user still has open. Called whenever
    // their password changes or their account is deactivated, so an old email
    // can't be used afterwards. Call before _unitOfWork.CompleteAsync() so it's
    // saved in the same step.
    public static async Task CloseOpenAsync(IUnitOfWork unitOfWork, int userId)
    {
        var repo = unitOfWork.Repository<PasswordResetToken>();
        var open = await repo.ListAsync(new OpenPasswordResetTokensForUserSpecification(userId));
        foreach (var token in open)
        {
            token.UsedAtUtc = DateTime.UtcNow;
            repo.Update(token);
        }
    }
}
