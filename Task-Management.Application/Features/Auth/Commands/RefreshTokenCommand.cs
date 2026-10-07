using AutoMapper;
using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Auth.DTOs;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Auth;

namespace Task_Management.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
{
    public string RefreshToken { get; set; }

    public RefreshTokenCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private static readonly Error InvalidToken = new("Auth.InvalidRefreshToken", "The refresh token is invalid or expired.");

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJwtTokenService _jwt;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IJwtTokenService jwt)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwt = jwt;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result.Failure<AuthResponseDto>(InvalidToken);
        }

        var repo = _unitOfWork.Repository<RefreshToken>();
        var stored = await repo.GetEntityWithSpec(new RefreshTokenWithUserSpecification(request.RefreshToken));
        if (stored is null || !stored.IsActive || !stored.User.IsActive)
        {
            return Result.Failure<AuthResponseDto>(InvalidToken);
        }

        // Rotation: the old token is revoked and a new one is issued, so a
        // stolen/replayed refresh token stops working after its first use.
        stored.RevokedAtUtc = DateTime.UtcNow;
        repo.Update(stored);

        var access = _jwt.CreateAccessToken(stored.User);
        var refresh = _jwt.CreateRefreshToken();
        repo.Add(new RefreshToken
        {
            Token = refresh.Token,
            UserId = stored.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refresh.ExpiresAtUtc
        });
        await _unitOfWork.CompleteAsync();

        return Result.Success(new AuthResponseDto
        {
            AccessToken = access.Token,
            AccessTokenExpiresAtUtc = access.ExpiresAtUtc,
            RefreshToken = refresh.Token,
            RefreshTokenExpiresAtUtc = refresh.ExpiresAtUtc,
            MustChangePassword = stored.User.MustChangePassword,
            User = _mapper.Map<UserDto>(stored.User)
        });
    }
}
