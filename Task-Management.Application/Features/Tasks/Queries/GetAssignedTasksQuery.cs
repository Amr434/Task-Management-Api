using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Tasks;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tasks.Queries;

public class GetAssignedTasksQuery : IRequest<Result<IEnumerable<TaskItemDto>>>
{
    public int UserId { get; set; }

    public GetAssignedTasksQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetAssignedTasksQueryHandler : IRequestHandler<GetAssignedTasksQuery, Result<IEnumerable<TaskItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAssignedTasksQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TaskItemDto>>> Handle(GetAssignedTasksQuery request, CancellationToken cancellationToken)
    {
        var spec = new AssignedTasksWithDetailsSpecification(request.UserId);
        var tasks = await _unitOfWork.Repository<TaskItem>().ListAsync(spec);

        // The frontend consumes a flat list and rebuilds the tree from
        // ParentTaskId, so flatten the included subtasks into the result
        // (deduped: a subtask may itself be assigned to the user).
        var flat = new Dictionary<int, TaskItem>();
        foreach (var task in tasks)
        {
            AddWithSubtasks(task, flat);
        }

        var dtos = _mapper.Map<IEnumerable<TaskItemDto>>(flat.Values);
        return Result.Success(dtos);
    }

    private static void AddWithSubtasks(TaskItem task, Dictionary<int, TaskItem> flat)
    {
        if (!flat.TryAdd(task.Id, task)) return;
        foreach (var subtask in task.SubTasks)
        {
            AddWithSubtasks(subtask, flat);
        }
    }
}
