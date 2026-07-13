using AutoMapper;
using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Auth.DTOs;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Users;

namespace Task_Management.Application.Features.Auth.Commands;

// Admin-only: with no internet/SMTP there is no self-signup or email
// verification; an admin creates accounts and hands out temporary passwords.
public class RegisterUserCommand : IRequest<Result<UserDto>>
{
    public RegisterUserDto Dto { get; set; }

    public RegisterUserCommand(RegisterUserDto dto)
    {
        Dto = dto;
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasherService _hasher;

    public RegisterUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasherService hasher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hasher = hasher;
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
        if (!Enum.IsDefined(typeof(UserRole), dto.Role))
        {
            return Result.Failure<UserDto>(new Error("Auth.InvalidRole", "Role must be 0 (Member) or 1 (Admin)."));
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

        return Result.Success(_mapper.Map<UserDto>(user));
    }
}
