using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Comments;

namespace Task_Management.Application.Features.Comments.Queries;

// The number on the Replies badge: replies the user hasn't opened yet.
public class GetUnreadRepliesCountQuery : IRequest<Result<int>>
{
    public int UserId { get; set; }

    public GetUnreadRepliesCountQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetUnreadRepliesCountQueryHandler : IRequestHandler<GetUnreadRepliesCountQuery, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreadRepliesCountQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(GetUnreadRepliesCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _unitOfWork.Repository<Comment>()
            .CountAsync(new UnreadRepliesForUserSpecification(request.UserId));
        return Result.Success(count);
    }
}
