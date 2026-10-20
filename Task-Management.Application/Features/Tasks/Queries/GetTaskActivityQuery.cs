using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Comments;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Tasks;

namespace Task_Management.Application.Features.Tasks.Queries;

public class GetTaskActivityQuery : IRequest<Result<IEnumerable<TaskActivityDto>>>
{
    public int TaskId { get; set; }
    public int UserId { get; set; }

    public GetTaskActivityQuery(int taskId, int userId)
    {
        TaskId = taskId;
        UserId = userId;
    }
}

public class GetTaskActivityQueryHandler : IRequestHandler<GetTaskActivityQuery, Result<IEnumerable<TaskActivityDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTaskActivityQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TaskActivityDto>>> Handle(GetTaskActivityQuery request, CancellationToken cancellationToken)
    {
        // Same visibility rule as the task's comments: project participants only.
        var task = await _unitOfWork.Repository<TaskItem>().GetByIdAsync(request.TaskId);
        if (task is null || !await CommentAccess.CanAccessProject(_unitOfWork, task.ProjectId, request.UserId))
        {
            return Result.Failure<IEnumerable<TaskActivityDto>>(new Error("Task.NotFound", "Task not found or you don't have access to it."));
        }

        var activities = await _unitOfWork.Repository<TaskActivity>().ListAsync(new TaskActivitiesByTaskSpecification(request.TaskId));
        return Result.Success(_mapper.Map<IEnumerable<TaskActivityDto>>(activities));
    }
}
