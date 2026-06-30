using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Workspaces.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;

using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Workspaces.Queries;

public class GetWorkspaceByIdQuery : IRequest<Result<WorkspaceDto>>
{
    public int Id { get; set; }

    public GetWorkspaceByIdQuery(int id)
    {
        Id = id;
    }
}

public class GetWorkspaceByIdQueryHandler : IRequestHandler<GetWorkspaceByIdQuery, Result<WorkspaceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetWorkspaceByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<WorkspaceDto>> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        var workspace = await _unitOfWork.Repository<Workspace>().GetByIdAsync(request.Id);
        
        if (workspace == null)
        {
            return Result.Failure<WorkspaceDto>(new Error("Workspace.NotFound", $"Workspace with Id {request.Id} was not found."));
        }

        var dto = _mapper.Map<WorkspaceDto>(workspace);
        return Result.Success(dto);
    }
}
