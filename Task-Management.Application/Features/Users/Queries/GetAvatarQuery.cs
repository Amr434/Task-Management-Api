using MediatR;
using Task_Management.Application.Features.Attachments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Users.Queries;

// The picture file itself, streamed to <img>/<Image> tags.
public class GetAvatarQuery : IRequest<Result<AttachmentFileDto>>
{
    public int UserId { get; set; }

    public GetAvatarQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetAvatarQueryHandler : IRequestHandler<GetAvatarQuery, Result<AttachmentFileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public GetAvatarQueryHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task<Result<AttachmentFileDto>> Handle(GetAvatarQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
        var stream = string.IsNullOrEmpty(user?.AvatarPath) ? null : _storage.OpenRead(user.AvatarPath);
        if (user is null || stream is null)
        {
            return Result.Failure<AttachmentFileDto>(new Error("Avatar.NotFound", "No profile picture."));
        }

        return Result.Success(new AttachmentFileDto
        {
            Content = stream,
            ContentType = AvatarUrls.ContentTypeFor(user.AvatarPath!),
            FileName = Path.GetFileName(user.AvatarPath!)
        });
    }
}
