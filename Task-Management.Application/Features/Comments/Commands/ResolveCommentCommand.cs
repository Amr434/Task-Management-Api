using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Commands;

// Resolve (or reopen) an assigned comment. Any project participant may do
// this, matching ClickUp — not just the assignee.
public class ResolveCommentCommand : IRequest<Result<CommentDto>>
{
    public int CommentId { get; set; }
    public int UserId { get; set; }
    public bool Resolved { get; set; }

    public ResolveCommentCommand(int commentId, int userId, bool resolved)
    {
        CommentId = commentId;
        UserId = userId;
        Resolved = resolved;
    }
}

public class ResolveCommentCommandHandler : IRequestHandler<ResolveCommentCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ResolveCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CommentDto>> Handle(ResolveCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<Comment>()
            .GetEntityWithSpec(new CommentByIdWithDetailsSpecification(request.CommentId));
        if (comment?.TaskItem is null
            || !await CommentAccess.CanAccessProject(_unitOfWork, comment.TaskItem.ProjectId, request.UserId))
        {
            return Result.Failure<CommentDto>(new Error("Comment.NotFound", "Comment not found or you don't have access to it."));
        }

        if (request.Resolved)
        {
            comment.ResolvedById = request.UserId;
            comment.ResolvedAt = DateTime.UtcNow;
        }
        else
        {
            comment.ResolvedById = null;
            comment.ResolvedAt = null;
        }

        _unitOfWork.Repository<Comment>().Update(comment);
        await _unitOfWork.CompleteAsync();

        var saved = await _unitOfWork.Repository<Comment>()
            .GetEntityWithSpec(new CommentByIdWithDetailsSpecification(comment.Id));
        return Result.Success(_mapper.Map<CommentDto>(saved));
    }
}
