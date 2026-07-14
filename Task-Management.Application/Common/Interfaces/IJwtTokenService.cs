using Task_Management.Domain.Entities;

namespace Task_Management.Application.Common.Interfaces;

public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
public record RefreshTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(User user);
    RefreshTokenResult CreateRefreshToken();
}
