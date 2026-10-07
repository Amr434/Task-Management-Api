using Microsoft.AspNetCore.Identity;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Services;

// Wraps ASP.NET Core Identity's PasswordHasher (PBKDF2 with per-password salt)
// without pulling in the full Identity framework. Fully offline.
public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password)
        => _hasher.HashPassword(user, password);

    public bool Verify(User user, string passwordHash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user, passwordHash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
