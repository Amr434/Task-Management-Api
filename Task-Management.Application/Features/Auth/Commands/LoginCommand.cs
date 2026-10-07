using AutoMapper;
using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Auth.DTOs;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Users;

namespace Task_Management.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public LoginRequestDto Dto { get; set; }

    public LoginCommand(LoginRequestDto dto)
    {
        Dto = dto;
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    // Deliberately identical error for unknown email and wrong password,
    // so login responses don't reveal which emails exist.
    private static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password.");
    private static readonly Error Inactive = new("Auth.InactiveUser", "This account has been deactivated.");

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IJwtTokenService _jwt;
    private readonly IPasswordHasherService _hasher;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IJwtTokenService jwt, IPasswordHasherService hasher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Dto.Email.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Dto.Password))
        {
            return Result.Failure<AuthResponseDto>(InvalidCredentials);
        }

        var user = await _unitOfWork.Repository<User>().GetEntityWithSpec(new UserByEmailSpecification(email));
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return Result.Failure<AuthResponseDto>(InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthResponseDto>(Inactive);
        }

        if (!_hasher.Verify(user, user.PasswordHash, request.Dto.Password))
        {
            return Result.Failure<AuthResponseDto>(InvalidCredentials);
        }

        var access = _jwt.CreateAccessToken(user);
        var refresh = _jwt.CreateRefreshToken();

        _unitOfWork.Repository<RefreshToken>().Add(new RefreshToken
        {
            Token = refresh.Token,
            UserId = user.Id,
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
            MustChangePassword = user.MustChangePassword,
            User = _mapper.Map<UserDto>(user)
        });
    }
}
