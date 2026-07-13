using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Auth.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Auth.Commands;

public class ChangePasswordCommand : IRequest<Result<bool>>
{
    public int UserId { get; set; }
    public ChangePasswordDto Dto { get; set; }

    public ChangePasswordCommand(int userId, ChangePasswordDto dto)
    {
        UserId = userId;
        Dto = dto;
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _hasher;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasherService hasher)
    {
        _unitOfWork = unitOfWork;
        _hasher = hasher;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Dto.NewPassword) || request.Dto.NewPassword.Length < 8)
        {
            return Result.Failure<bool>(new Error("Auth.WeakPassword", "New password must be at least 8 characters."));
        }

        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<bool>(new Error("Auth.UserNotFound", "User not found."));
        }

        if (!_hasher.Verify(user, user.PasswordHash, request.Dto.CurrentPassword))
        {
            return Result.Failure<bool>(new Error("Auth.InvalidCredentials", "Current password is incorrect."));
        }

        user.PasswordHash = _hasher.Hash(user, request.Dto.NewPassword);
        user.MustChangePassword = false;
        repo.Update(user);
        await _unitOfWork.CompleteAsync();

        return Result.Success(true);
    }
}
