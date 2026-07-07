using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Tags.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Tags.Queries;

public class GetAllTagsQuery : IRequest<Result<IEnumerable<TagDto>>> { }

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, Result<IEnumerable<TagDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllTagsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<TagDto>>> Handle(GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _unitOfWork.Repository<Tag>().ListAllAsync();
        var dtos = _mapper.Map<IEnumerable<TagDto>>(tags);
        return Result.Success(dtos);
    }
}
