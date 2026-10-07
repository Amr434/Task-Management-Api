using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Users;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Auth;

namespace Task_Management.Application.Features.Auth.Commands;

// The page the emailed link opens: sets a new password using the link's token.
// The token works once, for a limited time, and only for an active account.
public class ResetPasswordCommand : IRequest<Result<bool>>
{
    public string Token { get; set; }
    public string NewPassword { get; set; }

    public ResetPasswordCommand(string token, string newPassword)
    {
        Token = token;
        NewPassword = newPassword;
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    // One message for every way a link can be wrong, so it gives nothing away.
    private static readonly Error InvalidLink =
        new("Auth.InvalidResetLink", "This reset link is invalid or has expired. Please request a new one.");

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _hasher;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasherService hasher)
    {
        _unitOfWork = unitOfWork;
        _hasher = hasher;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Result.Failure<bool>(new Error("Auth.WeakPassword", "New password must be at least 8 characters."));
        }

        var rawToken = (request.Token ?? string.Empty).Trim();
        if (rawToken.Length == 0)
        {
            return Result.Failure<bool>(InvalidLink);
        }

        var tokens = _unitOfWork.Repository<PasswordResetToken>();
        var token = await tokens.GetEntityWithSpec(
            new PasswordResetTokenByHashSpecification(PasswordResetTokens.Hash(rawToken)));

        var now = DateTime.UtcNow;
        if (token is null || token.UsedAtUtc != null || token.ExpiresAtUtc <= now
            || token.User is null || !token.User.IsActive)
        {
            return Result.Failure<bool>(InvalidLink);
        }

        var user = token.User;
        user.PasswordHash = _hasher.Hash(user, request.NewPassword);
        // They just chose this password themselves, so no forced change at next login.
        user.MustChangePassword = false;
        _unitOfWork.Repository<User>().Update(user);

        // This link and any other still-open link for the account stop working.
        await PasswordResetTokens.CloseOpenAsync(_unitOfWork, user.Id);

        // Sign the account out everywhere: whoever had the old password loses access.
        await UserManagementRules.RevokeRefreshTokensAsync(_unitOfWork, user.Id);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
