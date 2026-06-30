using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Workspaces.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Workspaces.Queries;

public class GetWorkspacesQuery : IRequest<Result<IEnumerable<WorkspaceDto>>> { }

public class GetWorkspacesQueryHandler : IRequestHandler<GetWorkspacesQuery, Result<IEnumerable<WorkspaceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetWorkspacesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WorkspaceDto>>> Handle(GetWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var workspaces = await _unitOfWork.Repository<Workspace>().ListAllAsync();
        var dtos = _mapper.Map<IEnumerable<WorkspaceDto>>(workspaces);
        return Result.Success(dtos);
    }
}
