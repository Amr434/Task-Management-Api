using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Comments.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Queries;

// All comments on tasks in projects shared with the user (plus comments assigned to them), newest first.
public class GetRepliesQuery : IRequest<Result<IEnumerable<CommentDto>>>
{
    public const int MaxResults = 100;

    public int UserId { get; set; }

    public GetRepliesQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetRepliesQueryHandler : IRequestHandler<GetRepliesQuery, Result<IEnumerable<CommentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRepliesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<CommentDto>>> Handle(GetRepliesQuery request, CancellationToken cancellationToken)
    {
        var comments = await _unitOfWork.Repository<Comment>()
            .ListAsync(new RepliesForUserSpecification(request.UserId, GetRepliesQuery.MaxResults));
        var dtos = _mapper.Map<List<CommentDto>>(comments);

        // Mark each reply read/unread for this user (drives the unread styling).
        var ids = comments.Select(c => c.Id).ToList();
        var reads = await _unitOfWork.Repository<CommentRead>()
            .ListAsync(new CommentReadsForUserSpecification(request.UserId, ids));
        var readIds = reads.Select(r => r.CommentId).ToHashSet();
        var ownIds = comments.Where(c => c.UserId == request.UserId).Select(c => c.Id).ToHashSet();
        foreach (var dto in dtos)
        {
            dto.IsRead = ownIds.Contains(dto.Id) || readIds.Contains(dto.Id);
        }

        return Result.Success<IEnumerable<CommentDto>>(dtos);
    }
}
