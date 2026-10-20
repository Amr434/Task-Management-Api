using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Users.Commands;

// Removes the current user's profile picture (they go back to initials).
public class RemoveAvatarCommand : IRequest<Result<UserDto>>
{
    public int UserId { get; set; }

    public RemoveAvatarCommand(int userId)
    {
        UserId = userId;
    }
}

public class RemoveAvatarCommandHandler : IRequestHandler<RemoveAvatarCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IMapper _mapper;

    public RemoveAvatarCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(RemoveAvatarCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("Users.NotFound", "User not found."));
        }

        var oldPath = user.AvatarPath;
        if (!string.IsNullOrEmpty(oldPath))
        {
            user.AvatarPath = null;
            repo.Update(user);
            await _unitOfWork.CompleteAsync();
            _storage.Delete(oldPath);
        }

        return Result.Success(_mapper.Map<UserDto>(user));
    }
}
