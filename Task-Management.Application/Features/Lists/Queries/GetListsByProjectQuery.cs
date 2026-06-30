using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Lists.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Specifications.Lists;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Lists.Queries;

public class GetListsByProjectQuery : IRequest<Result<IEnumerable<ListDto>>>
{
    public int ProjectId { get; set; }

    public GetListsByProjectQuery(int projectId)
    {
        ProjectId = projectId;
    }
}

public class GetListsByProjectQueryHandler : IRequestHandler<GetListsByProjectQuery, Result<IEnumerable<ListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetListsByProjectQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ListDto>>> Handle(GetListsByProjectQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListsByProjectSpecification(request.ProjectId);
        var lists = await _unitOfWork.Repository<List>().ListAsync(spec);
        
        var dtos = _mapper.Map<IEnumerable<ListDto>>(lists);
        return Result.Success(dtos);
    }
}
