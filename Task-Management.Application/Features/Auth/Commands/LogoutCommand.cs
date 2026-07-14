using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Auth;

namespace Task_Management.Application.Features.Auth.Commands;

public class LogoutCommand : IRequest<Result<bool>>
{
    public string RefreshToken { get; set; }

    public LogoutCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Logout always succeeds from the client's point of view; revoking the
        // refresh token is best-effort (it may already be gone or expired).
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var repo = _unitOfWork.Repository<RefreshToken>();
            var stored = await repo.GetEntityWithSpec(new RefreshTokenWithUserSpecification(request.RefreshToken));
            if (stored is not null && stored.RevokedAtUtc == null)
            {
                stored.RevokedAtUtc = DateTime.UtcNow;
                repo.Update(stored);
                await _unitOfWork.CompleteAsync();
            }
        }

        return Result.Success(true);
    }
}
