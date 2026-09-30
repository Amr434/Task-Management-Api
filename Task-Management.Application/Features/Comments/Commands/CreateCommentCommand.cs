using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;
using Task_Management.Domain.Specifications.Projects;
using Task_Management.Domain.Specifications.Spaces;

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
    private readonly ICommentNotifier _notifier;

    public CreateCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICommentNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifier = notifier;
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
        var dto = _mapper.Map<CommentDto>(saved);

        await NotifyAsync(task.ProjectId, request.UserId, comment.AssignedToId, dto);

        return Result.Success(dto);
    }

    // Live notification for everyone who can see this comment in Replies:
    // the space owner, space members, people the project is shared with, and
    // the person the comment is assigned to. Never the author. A failure here
    // must not undo the saved comment, so it's swallowed.
    private async Task NotifyAsync(int projectId, int authorId, int? assignedToId, CommentDto dto)
    {
        try
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetEntityWithSpec(new ProjectWithMembersSpecification(projectId));
            if (project is null) return;
            var space = await _unitOfWork.Repository<Space>()
                .GetEntityWithSpec(new SpaceWithMembersSpecification(project.SpaceId));

            var recipients = new HashSet<int>();
            if (space?.OwnerId is int ownerId) recipients.Add(ownerId);
            foreach (var m in space?.Members ?? Enumerable.Empty<User>()) recipients.Add(m.Id);
            foreach (var m in project.Members) recipients.Add(m.Id);
            if (assignedToId is int assignee) recipients.Add(assignee);
            recipients.Remove(authorId);

            await _notifier.CommentAddedAsync(recipients, dto);
        }
        catch
        {
            // Best effort: the comment is saved; the Replies list still shows it.
        }
    }
}
