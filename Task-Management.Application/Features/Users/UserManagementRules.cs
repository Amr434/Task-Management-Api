using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Auth;
using Task_Management.Application.Features.Users.DTOs;

namespace Task_Management.Application.Features.Users;

// The single place that decides who may manage whom:
//   SuperAdmin -> Admins and Members
//   Admin      -> Members only
//   Member     -> nobody
// Nobody can manage a SuperAdmin, and nobody can manage their own account here.
public static class UserManagementRules
{
    public static bool CanManage(UserRole actorRole, UserRole targetRole) => actorRole switch
    {
        UserRole.SuperAdmin => targetRole is UserRole.Admin or UserRole.Member,
        UserRole.Admin => targetRole == UserRole.Member,
        _ => false
    };

    public static bool CanManage(User actor, User target) =>
        actor.Id != target.Id && CanManage(actor.Role, target.Role);

    // Roles are read from the database, not from the JWT, so a demoted or
    // deactivated admin loses their powers immediately, not when the token expires.
    public static async Task<Result<User>> LoadActiveManagerAsync(IUnitOfWork unitOfWork, int actorId)
    {
        var actor = await unitOfWork.Repository<User>().GetByIdAsync(actorId);
        if (actor is null || !actor.IsActive || (actor.Role != UserRole.Admin && actor.Role != UserRole.SuperAdmin))
        {
            return Result.Failure<User>(new Error("Users.Forbidden", "You are not allowed to manage users."));
        }
        return Result.Success(actor);
    }

    // Loads the target and checks the actor may manage them.
    public static async Task<Result<User>> LoadManageableTargetAsync(IUnitOfWork unitOfWork, User actor, int targetId)
    {
        var target = await unitOfWork.Repository<User>().GetByIdAsync(targetId);
        if (target is null)
        {
            return Result.Failure<User>(new Error("Users.NotFound", "User not found."));
        }
        if (!CanManage(actor, target))
        {
            return Result.Failure<User>(new Error("Users.Forbidden", "You are not allowed to manage this user."));
        }
        return Result.Success(target);
    }

    // Signs the user out of every device: their access token still works until
    // it expires (max 60 min), but it can no longer be refreshed.
    // Call before _unitOfWork.CompleteAsync() so it's saved in the same step.
    public static async Task RevokeRefreshTokensAsync(IUnitOfWork unitOfWork, int userId)
    {
        var repo = unitOfWork.Repository<RefreshToken>();
        var tokens = await repo.ListAsync(new ActiveRefreshTokensForUserSpecification(userId));
        foreach (var token in tokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            repo.Update(token);
        }
    }

    public static ManagedUserDto ToManagedDto(User user, User actor) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = (int)user.Role,
        IsActive = user.IsActive,
        MustChangePassword = user.MustChangePassword,
        AvatarUrl = AvatarUrls.For(user),
        CanManage = CanManage(actor, user)
    };
}
