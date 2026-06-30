using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Tasks;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tasks.Queries;

public class GetTasksByListQuery : IRequest<Result<IEnumerable<TaskItemDto>>>
{
    public int ListId { get; set; }

    public GetTasksByListQuery(int listId)
    {
        ListId = listId;
    }
}

public class GetTasksByListQueryHandler : IRequestHandler<GetTasksByListQuery, Result<IEnumerable<TaskItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetTasksByListQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TaskItemDto>>> Handle(GetTasksByListQuery request, CancellationToken cancellationToken)
    {
        var spec = new TasksWithDetailsSpecification(request.ListId);
        var tasks = await _unitOfWork.Repository<TaskItem>().ListAsync(spec);
        
        var dtos = _mapper.Map<IEnumerable<TaskItemDto>>(tasks);
        return Result.Success(dtos);
    }
}
