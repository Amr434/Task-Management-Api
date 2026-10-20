using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Users.Commands;

// Any signed-in user sets their own profile picture. The old file is deleted.
public class UploadAvatarCommand : IRequest<Result<UserDto>>
{
    public int UserId { get; set; }
    public string ContentType { get; set; }
    public long Length { get; set; }
    public Stream Content { get; set; }

    public UploadAvatarCommand(int userId, string contentType, long length, Stream content)
    {
        UserId = userId;
        ContentType = contentType;
        Length = length;
        Content = content;
    }
}

public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IMapper _mapper;

    public UploadAvatarCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        if (!AvatarUrls.ExtensionsByContentType.TryGetValue(request.ContentType ?? string.Empty, out var extension))
        {
            return Result.Failure<UserDto>(new Error("Avatar.InvalidType", "Profile picture must be a JPG, PNG, WEBP, GIF or HEIC image."));
        }
        if (request.Length <= 0 || request.Length > AvatarUrls.MaxBytes)
        {
            return Result.Failure<UserDto>(new Error("Avatar.TooLarge", "Profile picture must be smaller than 5 MB."));
        }

        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error("Users.NotFound", "User not found."));
        }

        // A new unique name per upload, so browsers and phones never show a cached old picture.
        var newPath = Path.Combine("avatars", user.Id.ToString(), $"{Guid.NewGuid():N}{extension}");
        await _storage.SaveAsync(request.Content, newPath, cancellationToken);

        var oldPath = user.AvatarPath;
        user.AvatarPath = newPath;
        repo.Update(user);
        await _unitOfWork.CompleteAsync();

        if (!string.IsNullOrEmpty(oldPath))
        {
            _storage.Delete(oldPath);
        }

        return Result.Success(_mapper.Map<UserDto>(user));
    }
}
