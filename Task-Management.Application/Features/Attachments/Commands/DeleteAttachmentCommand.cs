using MediatR;
using Task_Management.Application.Features.Comments;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Attachments;

namespace Task_Management.Application.Features.Attachments.Commands;

// Any project participant can remove an attachment (attachments belong to
// the task, which every participant can edit).
public class DeleteAttachmentCommand : IRequest<Result<bool>>
{
    public int AttachmentId { get; set; }
    public int UserId { get; set; }

    public DeleteAttachmentCommand(int attachmentId, int userId)
    {
        AttachmentId = attachmentId;
        UserId = userId;
    }
}

public class DeleteAttachmentCommandHandler : IRequestHandler<DeleteAttachmentCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public DeleteAttachmentCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task<Result<bool>> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _unitOfWork.Repository<Attachment>()
            .GetEntityWithSpec(new AttachmentByIdWithTaskSpecification(request.AttachmentId));
        if (attachment?.TaskItem is null
            || !await CommentAccess.CanAccessProject(_unitOfWork, attachment.TaskItem.ProjectId, request.UserId))
        {
            return Result.Failure<bool>(new Error("Attachment.NotFound", "Attachment not found or you don't have access to it."));
        }

        _unitOfWork.Repository<Attachment>().Delete(attachment);
        await _unitOfWork.CompleteAsync();

        // Remove the file after the row is gone; a leftover file is harmless,
        // a dangling row pointing at nothing is not.
        _storage.Delete(attachment.FileUrl);
        return Result.Success(true);
    }
}
