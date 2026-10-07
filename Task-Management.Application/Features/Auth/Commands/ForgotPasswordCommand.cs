using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Auth;
using Task_Management.Domain.Specifications.Users;

namespace Task_Management.Application.Features.Auth.Commands;

// "Forgot password" on the login page: emails the user a link to choose a new
// password. Always succeeds, whether or not the email belongs to an account,
// so the response never reveals which emails exist.
public class ForgotPasswordCommand : IRequest<Result<bool>>
{
    public string Email { get; set; }

    public ForgotPasswordCommand(string email)
    {
        Email = email;
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountNotifier _notifier;

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IAccountNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = (request.Email ?? string.Empty).Trim();
        if (email.Length == 0)
        {
            return Result.Success(true);
        }

        var user = await _unitOfWork.Repository<User>().GetEntityWithSpec(new UserByEmailSpecification(email));
        // Unknown email or deactivated account: say nothing, send nothing.
        if (user is null || !user.IsActive)
        {
            return Result.Success(true);
        }

        // From here on nothing may fail the request: an error for real accounts
        // only would reveal which emails exist.
        try
        {
            var now = DateTime.UtcNow;
            var tokens = _unitOfWork.Repository<PasswordResetToken>();
            var open = await tokens.ListAsync(new OpenPasswordResetTokensForUserSpecification(user.Id));
            var live = open.Where(t => t.ExpiresAtUtc > now).ToList();

            // A link was sent a moment ago, or enough are already waiting in the
            // inbox: don't send another. Earlier links keep working until they
            // expire, so repeated requests by someone else can't cancel them.
            if (live.Any(t => t.CreatedAtUtc > now.AddSeconds(-PasswordResetTokens.ResendCooldownSeconds))
                || live.Count >= PasswordResetTokens.MaxOpenLinks)
            {
                return Result.Success(true);
            }

            var token = PasswordResetTokens.NewToken();
            tokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = PasswordResetTokens.Hash(token),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(PasswordResetTokens.LifetimeMinutes)
            });
            await _unitOfWork.CompleteAsync();

            await _notifier.PasswordResetRequestedAsync(user.Email, user.FirstName, token, PasswordResetTokens.LifetimeMinutes);
        }
        catch
        {
            // Same answer as success; the user can simply ask again.
        }

        return Result.Success(true);
    }
}
