using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Application.Features.Auth;

namespace Task_Management.Application.Features.Users.Commands;

// Activate or deactivate a user. Super Admin: Admins and Members.
// Admin: Members only. A deactivated user can't log in and is signed out.
public class SetUserStatusCommand : IRequest<Result<ManagedUserDto>>
{
    public int ActorId { get; set; }
    public int TargetUserId { get; set; }
    public bool IsActive { get; set; }

    public SetUserStatusCommand(int actorId, int targetUserId, bool isActive)
    {
        ActorId = actorId;
        TargetUserId = targetUserId;
        IsActive = isActive;
    }
}

public class SetUserStatusCommandHandler : IRequestHandler<SetUserStatusCommand, Result<ManagedUserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SetUserStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ManagedUserDto>> Handle(SetUserStatusCommand request, CancellationToken cancellationToken)
    {
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

        target.IsActive = request.IsActive;
        _unitOfWork.Repository<User>().Update(target);
        if (!request.IsActive)
        {
            await UserManagementRules.RevokeRefreshTokensAsync(_unitOfWork, target.Id);
            // ...and any emailed "forgot password" link stops working for good.
            await PasswordResetTokens.CloseOpenAsync(_unitOfWork, target.Id);
        }
        await _unitOfWork.CompleteAsync();

        return Result.Success(UserManagementRules.ToManagedDto(target, actor));
    }
}
