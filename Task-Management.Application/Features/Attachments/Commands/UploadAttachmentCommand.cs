using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Attachments.DTOs;
using Task_Management.Application.Features.Comments;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Attachments;

namespace Task_Management.Application.Features.Attachments.Commands;

public class UploadAttachmentCommand : IRequest<Result<AttachmentDto>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public Stream Content { get; set; } = Stream.Null;

    public UploadAttachmentCommand(int taskId, int userId, string fileName, string? contentType, long fileSize, Stream content)
    {
        TaskId = taskId;
        UserId = userId;
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        Content = content;
    }
}

public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, Result<AttachmentDto>>
{
    public const long MaxFileSize = 25 * 1024 * 1024; // 25 MB

    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IMapper _mapper;

    public UploadAttachmentCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _mapper = mapper;
    }

    public async Task<Result<AttachmentDto>> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskId);
        if (task is null || !await CommentAccess.CanAccessProject(_unitOfWork, task.ProjectId, request.UserId))
        {
            return Result.Failure<AttachmentDto>(new Error("Task.NotFound", "Task not found or you don't have access to it."));
        }

        if (request.FileSize <= 0 || request.FileSize > MaxFileSize)
        {
            return Result.Failure<AttachmentDto>(new Error("Attachment.TooLarge", $"Attachments must be between 1 byte and {MaxFileSize / (1024 * 1024)} MB."));
        }

        // Never trust the client's file name for storage: keep it only for
        // display and store under a generated name.
        var safeName = Path.GetFileName(request.FileName);
        if (string.IsNullOrWhiteSpace(safeName))
        {
            return Result.Failure<AttachmentDto>(new Error("Attachment.InvalidName", "Invalid file name."));
        }
        var storedPath = Path.Combine(request.TaskId.ToString(), $"{Guid.NewGuid():N}{Path.GetExtension(safeName)}");

        await _storage.SaveAsync(request.Content, storedPath, cancellationToken);

        var attachment = new Attachment
        {
            FileName = safeName,
            FileUrl = storedPath,
            FileSize = request.FileSize,
            ContentType = request.ContentType,
            TaskItemId = request.TaskId,
            UploadedById = request.UserId,
        };
        _unitOfWork.Repository<Attachment>().Add(attachment);
        await _unitOfWork.CompleteAsync();

        var saved = await _unitOfWork.Repository<Attachment>()
            .GetEntityWithSpec(new AttachmentByIdWithTaskSpecification(attachment.Id));
        return Result.Success(_mapper.Map<AttachmentDto>(saved));
    }
}
