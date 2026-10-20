using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Users.Commands;

// Super Admin only: promote a Member to Admin or demote an Admin to Member.
// Nobody can be made Super Admin through the API.
public class UpdateUserRoleCommand : IRequest<Result<ManagedUserDto>>
{
    public int ActorId { get; set; }
    public int TargetUserId { get; set; }
    public int NewRole { get; set; }

    public UpdateUserRoleCommand(int actorId, int targetUserId, int newRole)
    {
        ActorId = actorId;
        TargetUserId = targetUserId;
        NewRole = newRole;
    }
}

public class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, Result<ManagedUserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ManagedUserDto>> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (request.NewRole != (int)UserRole.Member && request.NewRole != (int)UserRole.Admin)
        {
            return Result.Failure<ManagedUserDto>(new Error("Users.InvalidRole", "Role must be 0 (Member) or 1 (Admin)."));
        }

        var actorResult = await UserManagementRules.LoadActiveManagerAsync(_unitOfWork, request.ActorId);
        if (actorResult.IsFailure)
        {
            return Result.Failure<ManagedUserDto>(actorResult.Error);
        }
        var actor = actorResult.Value;
        if (actor.Role != UserRole.SuperAdmin)
        {
            return Result.Failure<ManagedUserDto>(new Error("Users.Forbidden", "Only the Super Admin can change roles."));
        }

        var targetResult = await UserManagementRules.LoadManageableTargetAsync(_unitOfWork, actor, request.TargetUserId);
        if (targetResult.IsFailure)
        {
            return Result.Failure<ManagedUserDto>(targetResult.Error);
        }
        var target = targetResult.Value;

        target.Role = (UserRole)request.NewRole;
        _unitOfWork.Repository<User>().Update(target);
        await _unitOfWork.CompleteAsync();

        return Result.Success(UserManagementRules.ToManagedDto(target, actor));
    }
}
