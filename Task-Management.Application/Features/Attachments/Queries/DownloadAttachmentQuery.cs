using MediatR;
using Task_Management.Application.Features.Attachments.DTOs;
using Task_Management.Application.Features.Comments;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Attachments;

namespace Task_Management.Application.Features.Attachments.Queries;

public class DownloadAttachmentQuery : IRequest<Result<AttachmentFileDto>>
{
    public int AttachmentId { get; set; }
    public int UserId { get; set; }

    public DownloadAttachmentQuery(int attachmentId, int userId)
    {
        AttachmentId = attachmentId;
        UserId = userId;
    }
}

public class DownloadAttachmentQueryHandler : IRequestHandler<DownloadAttachmentQuery, Result<AttachmentFileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public DownloadAttachmentQueryHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task<Result<AttachmentFileDto>> Handle(DownloadAttachmentQuery request, CancellationToken cancellationToken)
    {
        var attachment = await _unitOfWork.Repository<Attachment>()
            .GetEntityWithSpec(new AttachmentByIdWithTaskSpecification(request.AttachmentId));
        if (attachment?.TaskItem is null
            || !await CommentAccess.CanAccessProject(_unitOfWork, attachment.TaskItem.ProjectId, request.UserId))
        {
            return Result.Failure<AttachmentFileDto>(new Error("Attachment.NotFound", "Attachment not found or you don't have access to it."));
        }

        var stream = _storage.OpenRead(attachment.FileUrl);
        if (stream is null)
        {
            return Result.Failure<AttachmentFileDto>(new Error("Attachment.FileMissing", "The attachment file is missing on the server."));
        }

        return Result.Success(new AttachmentFileDto
        {
            Content = stream,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType ?? "application/octet-stream",
        });
    }
}
