using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Queries;

public class GetTaskCommentsQuery : IRequest<Result<IEnumerable<CommentDto>>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }

    public GetTaskCommentsQuery(int taskId, int userId)
    {
        TaskId = taskId;
        UserId = userId;
    }
}

public class GetTaskCommentsQueryHandler : IRequestHandler<GetTaskCommentsQuery, Result<IEnumerable<CommentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTaskCommentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetTaskCommentsQuery request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskId);
        if (task is null || !await CommentAccess.CanAccessProject(_unitOfWork, task.ProjectId, request.UserId))
        {
            return Result.Failure<IEnumerable<CommentDto>>(new Error("Task.NotFound", "Task not found or you don't have access to it."));
        }

        var comments = await _unitOfWork.Repository<Comment>().ListAsync(new CommentsByTaskSpecification(request.TaskId));
        return Result.Success(_mapper.Map<IEnumerable<CommentDto>>(comments));
    }
}
