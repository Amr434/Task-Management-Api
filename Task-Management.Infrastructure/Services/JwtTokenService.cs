using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Services;

// Issues locally-signed JWTs (HMAC-SHA256). Validation is pure signature
// math against the shared key from config — no network calls, so it works
// on an offline LAN.
public class JwtTokenService : IJwtTokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public JwtTokenService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured. Add a random secret of at least 32 characters to appsettings.");
        if (_key.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters for HMAC-SHA256.");
        }
        _issuer = configuration["Jwt:Issuer"] ?? "TaskManagement";
        _audience = configuration["Jwt:Audience"] ?? "TaskManagement";
        _accessTokenMinutes = int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 60;
        _refreshTokenDays = int.TryParse(configuration["Jwt:RefreshTokenDays"], out var d) ? d : 14;
    }

    public AccessTokenResult CreateAccessToken(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public RefreshTokenResult CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return new RefreshTokenResult(
            Convert.ToBase64String(bytes),
            DateTime.UtcNow.AddDays(_refreshTokenDays));
    }
}
