using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Commands;

// Marks replies as read for the current user, which lowers the Replies badge:
//  - CommentId set: that one comment (clicking it in Replies or in a notification);
//  - TaskId set:    every comment on that task (opening the task);
//  - neither:       all replies ("Mark all as read").
// Only comments the user can see are affected; their own are always "read".
// Returns how many comments were newly marked.
public class MarkRepliesReadCommand : IRequest<Result<int>>
{
    public int UserId { get; set; }
    public int? CommentId { get; set; }
    public int? TaskId { get; set; }

    public MarkRepliesReadCommand(int userId, int? commentId = null, int? taskId = null)
    {
        UserId = userId;
        CommentId = commentId;
        TaskId = taskId;
    }
}

public class MarkRepliesReadCommandHandler : IRequestHandler<MarkRepliesReadCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkRepliesReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(MarkRepliesReadCommand request, CancellationToken cancellationToken)
    {
        int? taskId = request.TaskId;

        if (request.CommentId is int commentId)
        {
            var comment = await _unitOfWork.Repository<Comment>().GetByIdAsync(commentId);
            if (comment is null)
            {
                return Result.Failure<int>(new Error("Comment.NotFound", "Comment not found or you don't have access to it."));
            }
            taskId = comment.TaskItemId;
        }

        if (taskId is int id)
        {
            var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(id);
            if (task is null || !await CommentAccess.CanAccessProject(_unitOfWork, task.ProjectId, request.UserId))
            {
                return Result.Failure<int>(new Error("Task.NotFound", "Task not found or you don't have access to it."));
            }
        }

        var marked = await CommentReads.MarkAsync(_unitOfWork, request.UserId,
            new UnreadRepliesForUserSpecification(request.UserId, taskId, request.CommentId));
        return Result.Success(marked);
    }
}
