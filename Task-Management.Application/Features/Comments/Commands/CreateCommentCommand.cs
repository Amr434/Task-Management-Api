using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Commands;

public class CreateCommentCommand : IRequest<Result<CommentDto>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }
    public CreateCommentDto CommentDto { get; set; }

    public CreateCommentCommand(int taskId, int userId, CreateCommentDto commentDto)
    {
        TaskId = taskId;
        UserId = userId;
        CommentDto = commentDto;
    }
}

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(v => v.CommentDto.Text)
            .NotEmpty().WithMessage("Comment text is required.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskId);
        if (task is null || !await CommentAccess.CanAccessProject(_unitOfWork, task.ProjectId, request.UserId))
        {
            return Result.Failure<CommentDto>(new Error("Task.NotFound", "Task not found or you don't have access to it."));
        }

        // An assigned comment must target a participant of the same project
        // (which includes the author themselves).
        if (request.CommentDto.AssignedToId is int assigneeId
            && !await CommentAccess.CanAccessProject(_unitOfWork, task.ProjectId, assigneeId))
        {
            return Result.Failure<CommentDto>(new Error("Comment.InvalidAssignee", "The assignee must be a participant in this project."));
        }

        var comment = new Comment
        {
            Text = request.CommentDto.Text.Trim(),
            TaskItemId = request.TaskId,
            UserId = request.UserId,
            AssignedToId = request.CommentDto.AssignedToId,
        };

        _unitOfWork.Repository<Comment>().Add(comment);
        await _unitOfWork.CompleteAsync();

        // Reload with the people/task included so the DTO comes back complete.
        var saved = await _unitOfWork.Repository<Comment>()
            .GetEntityWithSpec(new CommentByIdWithDetailsSpecification(comment.Id));
        return Result.Success(_mapper.Map<CommentDto>(saved));
    }
}
