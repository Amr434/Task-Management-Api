using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Tasks;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tasks.Queries;

public class GetTasksByProjectQuery : IRequest<Result<IEnumerable<TaskItemDto>>>
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }

    public GetTasksByProjectQuery(int projectId, int userId)
    {
        ProjectId = projectId;
        UserId = userId;
    }
}

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, Result<IEnumerable<TaskItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTasksByProjectQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TaskItemDto>>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        // Tasks are visible only with project access (space owner/member or direct share).
        var access = await _unitOfWork.Repository<Project>()
            .GetEntityWithSpec(new Task_Management.Domain.Specifications.Projects.AccessibleProjectSpecification(request.ProjectId, request.UserId));
        if (access is null)
        {
            return Result.Failure<IEnumerable<TaskItemDto>>(new Error("Project.NotFound", "Project not found or you don't have access to it."));
        }

        var spec = new TasksWithDetailsSpecification(request.ProjectId);
        var tasks = await _unitOfWork.Repository<TaskItem>().ListAsync(spec);
        
        var dtos = _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
        return Result.Success(dtos);
    }
}
