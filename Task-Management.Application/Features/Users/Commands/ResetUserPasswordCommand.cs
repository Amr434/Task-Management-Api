using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Application.Features.Auth;

namespace Task_Management.Application.Features.Users.Commands;

// Set a temporary password for a user (e.g. they forgot theirs). They must
// change it on their next login, and they're signed out everywhere.
// Super Admin: Admins and Members. Admin: Members only.
public class ResetUserPasswordCommand : IRequest<Result<ManagedUserDto>>
{
    public int ActorId { get; set; }
    public int TargetUserId { get; set; }
    public string NewPassword { get; set; }

    public ResetUserPasswordCommand(int actorId, int targetUserId, string newPassword)
    {
        ActorId = actorId;
        TargetUserId = targetUserId;
        NewPassword = newPassword;
    }
}

public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, Result<ManagedUserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _hasher;

    public ResetUserPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasherService hasher)
    {
        _unitOfWork = unitOfWork;
        _hasher = hasher;
    }

    public async Task<Result<ManagedUserDto>> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Result.Failure<ManagedUserDto>(new Error("Auth.WeakPassword", "Password must be at least 8 characters."));
        }

        var actorResult = await UserManagementRules.LoadActiveManagerAsync(_unitOfWork, request.ActorId);
        if (actorResult.IsFailure)
        {
            return Result.Failure<ManagedUserDto>(actorResult.Error);
        }
        var actor = actorResult.Value;

        var targetResult = await UserManagementRules.LoadManageableTargetAsync(_unitOfWork, actor, request.TargetUserId);
        if (targetResult.IsFailure)
        {
            return Result.Failure<ManagedUserDto>(targetResult.Error);
        }
        var target = targetResult.Value;

        target.PasswordHash = _hasher.Hash(target, request.NewPassword);
        target.MustChangePassword = true;
        _unitOfWork.Repository<User>().Update(target);
        await UserManagementRules.RevokeRefreshTokensAsync(_unitOfWork, target.Id);
        // An emailed "forgot password" link must not override the admin's reset.
        await PasswordResetTokens.CloseOpenAsync(_unitOfWork, target.Id);
        await _unitOfWork.CompleteAsync();

        return Result.Success(UserManagementRules.ToManagedDto(target, actor));
    }
}
