using AutoMapper;
using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Auth.DTOs;
using Task_Management.Application.Features.Users;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Users;

namespace Task_Management.Application.Features.Auth.Commands;

// Admin-only: there is no self-signup; an admin creates accounts with a
// temporary password, which is also emailed to the new user.
// SuperAdmin can create Admins and Members; Admin can create Members only.
public class RegisterUserCommand : IRequest<Result<UserDto>>
{
    public RegisterUserDto Dto { get; set; }
    public int ActorId { get; set; }

    public RegisterUserCommand(RegisterUserDto dto, int actorId)
    {
        Dto = dto;
        ActorId = actorId;
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasherService _hasher;
    private readonly IAccountNotifier _notifier;

    public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasherService hasher, IAccountNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hasher = hasher;
        _notifier = notifier;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var email = dto.Email.Trim();

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<UserDto>(new Error("Auth.InvalidUser", "First name and email are required."));
        }
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
        {
            return Result.Failure<UserDto>(new Error("Auth.WeakPassword", "Password must be at least 8 characters."));
        }
        if (dto.Role != (int)UserRole.Member && dto.Role != (int)UserRole.Admin)
        {
            return Result.Failure<UserDto>(new Error("Auth.InvalidRole", "Role must be 0 (Member) or 1 (Admin)."));
        }

        var actorResult = await UserManagementRules.LoadActiveManagerAsync(_unitOfWork, request.ActorId);
        if (actorResult.IsFailure)
        {
            return Result.Failure<UserDto>(actorResult.Error);
        }
        if (!UserManagementRules.CanManage(actorResult.Value.Role, (UserRole)dto.Role))
        {
            return Result.Failure<UserDto>(new Error("Users.Forbidden", "Only the Super Admin can create Admin accounts."));
        }

        var repo = _unitOfWork.Repository<User>();
        var existing = await repo.GetEntityWithSpec(new UserByEmailSpecification(email));
        if (existing is not null)
        {
            return Result.Failure<UserDto>(new Error("Auth.EmailTaken", "A user with this email already exists."));
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = email,
            Role = (UserRole)dto.Role,
            IsActive = true,
            MustChangePassword = true
        };
        user.PasswordHash = _hasher.Hash(user, dto.Password);

        repo.Add(user);
        await _unitOfWork.CompleteAsync();

        // Welcome email; failing to send it must not fail the account creation.
        try
        {
            var actor = actorResult.Value;
            var actorName = $"{actor.FirstName} {actor.LastName}".Trim();
            await _notifier.AccountCreatedAsync(user.Email, user.FirstName,
                string.IsNullOrEmpty(actorName) ? "An admin" : actorName, dto.Password);
        }
        catch
        {
            // The admin can still hand the details over themselves.
        }

        return Result.Success(_mapper.Map<UserDto>(user));
    }
}
