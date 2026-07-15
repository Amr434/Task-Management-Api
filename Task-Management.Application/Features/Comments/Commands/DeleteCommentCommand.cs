using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Commands;

// Only the author can delete their comment.
public class DeleteCommentCommand : IRequest<Result<bool>>
{
    public int CommentId { get; set; }
    public int UserId { get; set; }

    public DeleteCommentCommand(int commentId, int userId)
    {
        CommentId = commentId;
        UserId = userId;
    }
}

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Repository<Comment>()
            .GetEntityWithSpec(new CommentByIdWithDetailsSpecification(request.CommentId));
        if (comment?.TaskItem is null
            || !await CommentAccess.CanAccessProject(_unitOfWork, comment.TaskItem.ProjectId, request.UserId))
        {
            return Result.Failure<bool>(new Error("Comment.NotFound", "Comment not found or you don't have access to it."));
        }

        if (comment.UserId != request.UserId)
        {
            return Result.Failure<bool>(new Error("Comment.Forbidden", "Only the author can delete a comment."));
        }

        _unitOfWork.Repository<Comment>().Delete(comment);
        await _unitOfWork.CompleteAsync();
        return Result.Success(true);
    }
}
