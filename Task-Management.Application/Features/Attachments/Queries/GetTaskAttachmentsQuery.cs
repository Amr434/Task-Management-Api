using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Attachments.DTOs;
using Task_Management.Application.Features.Comments;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Attachments;

namespace Task_Management.Application.Features.Attachments.Queries;

public class GetTaskAttachmentsQuery : IRequest<Result<IEnumerable<AttachmentDto>>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }

    public GetTaskAttachmentsQuery(int taskId, int userId)
    {
        TaskId = taskId;
        UserId = userId;
    }
}

public class GetTaskAttachmentsQueryHandler : IRequestHandler<GetTaskAttachmentsQuery, Result<IEnumerable<AttachmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTaskAttachmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<AttachmentDto>>> Handle(GetTaskAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskId);
        if (task is null || !await CommentAccess.CanAccessProject(_unitOfWork, task.ProjectId, request.UserId))
        {
            return Result.Failure<IEnumerable<AttachmentDto>>(new Error("Task.NotFound", "Task not found or you don't have access to it."));
        }

        var attachments = await _unitOfWork.Repository<Attachment>().ListAsync(new AttachmentsByTaskSpecification(request.TaskId));
        return Result.Success(_mapper.Map<IEnumerable<AttachmentDto>>(attachments));
    }
}
