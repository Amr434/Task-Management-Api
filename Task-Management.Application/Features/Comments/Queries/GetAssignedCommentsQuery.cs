using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Queries;

// The current user's action items: comments assigned to them, anywhere.
public class GetAssignedCommentsQuery : IRequest<Result<IEnumerable<CommentDto>>>
{
    public int UserId { get; set; }

    public GetAssignedCommentsQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetAssignedCommentsQueryHandler : IRequestHandler<GetAssignedCommentsQuery, Result<IEnumerable<CommentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAssignedCommentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetAssignedCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _unitOfWork.Repository<Comment>().ListAsync(new AssignedCommentsForUserSpecification(request.UserId));
        return Result.Success(_mapper.Map<IEnumerable<CommentDto>>(comments));
    }
}
