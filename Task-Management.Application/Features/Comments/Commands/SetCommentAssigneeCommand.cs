using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Commands;

// Assign, reassign, or (with null) unassign a comment.
public class SetCommentAssigneeCommand : IRequest<Result<CommentDto>>
{
    public int CommentId { get; set; }
    public int UserId { get; set; }
    public int? AssignedToId { get; set; }

    public SetCommentAssigneeCommand(int commentId, int userId, int? assignedToId)
    {
        CommentId = commentId;
        UserId = userId;
        AssignedToId = assignedToId;
    }
}

public class SetCommentAssigneeCommandHandler : IRequestHandler<SetCommentAssigneeCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SetCommentAssigneeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CommentDto>> Handle(SetCommentAssigneeCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<Comment>()
            .GetEntityWithSpec(new CommentByIdWithDetailsSpecification(request.CommentId));
        if (comment?.TaskItem is null
            || !await CommentAccess.CanAccessProject(_unitOfWork, comment.TaskItem.ProjectId, request.UserId))
        {
            return Result.Failure<CommentDto>(new Error("Comment.NotFound", "Comment not found or you don't have access to it."));
        }

        if (request.AssignedToId is int assigneeId
            && !await CommentAccess.CanAccessProject(_unitOfWork, comment.TaskItem.ProjectId, assigneeId))
        {
            return Result.Failure<CommentDto>(new Error("Comment.InvalidAssignee", "The assignee must be a participant in this project."));
        }

        comment.AssignedToId = request.AssignedToId;
        // Reassigning an open action item keeps it open; unassigning clears
        // nothing else — resolution history stays as-is.

        _unitOfWork.Repository<Comment>().Update(comment);
        await _unitOfWork.CompleteAsync();

        var saved = await _unitOfWork.Repository<Comment>()
            .GetEntityWithSpec(new CommentByIdWithDetailsSpecification(comment.Id));
        return Result.Success(_mapper.Map<CommentDto>(saved));
    }
}
