using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Spaces.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Spaces.Queries;

public class GetAllSpacesQuery : IRequest<Result<IEnumerable<SpaceDto>>>
{
    public int UserId { get; set; }

    public GetAllSpacesQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetAllSpacesQueryHandler : IRequestHandler<GetAllSpacesQuery, Result<IEnumerable<SpaceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSpacesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<SpaceDto>>> Handle(GetAllSpacesQuery request, CancellationToken cancellationToken)
    {
        // Only spaces the user owns or has been accepted into.
        var spec = new Task_Management.Domain.Specifications.Spaces.SpacesForUserSpecification(request.UserId);
        var spaces = await _unitOfWork.Repository<Space>().ListAsync(spec);
        
        var dtos = _mapper.Map<IEnumerable<SpaceDto>>(spaces);
        return Result.Success(dtos);
    }
}
