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

    public GetTasksByProjectQuery(int projectId)
    {
        ProjectId = projectId;
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
        var spec = new TasksWithDetailsSpecification(request.ProjectId);
        var tasks = await _unitOfWork.Repository<TaskItem>().ListAsync(spec);
        
        var dtos = _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
        return Result.Success(dtos);
    }
}
