using MediatR;
using Task_Management.Application.Features.Notifications.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Notifications;

namespace Task_Management.Application.Features.Notifications.Queries;

// The bell's list: the current user's notifications, newest first, a page at
// a time (pass the last id you have as BeforeId for the next page).
public class GetNotificationsQuery : IRequest<Result<IEnumerable<NotificationDto>>>
{
    public const int MaxPageSize = 50;

    public int UserId { get; set; }
    public int? BeforeId { get; set; }
    public int Take { get; set; }

    public GetNotificationsQuery(int userId, int? beforeId, int take)
    {
        UserId = userId;
        BeforeId = beforeId;
        Take = Math.Clamp(take, 1, MaxPageSize);
    }
}

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<IEnumerable<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Repository<Notification>()
            .ListAsync(new NotificationsForUserSpecification(request.UserId, request.BeforeId, request.Take));
        return Result.Success(items.Select(NotificationMapping.ToDto).ToList().AsEnumerable());
    }
}

// The number on the bell.
public class GetUnreadNotificationCountQuery : IRequest<Result<int>>
{
    public int UserId { get; set; }

    public GetUnreadNotificationCountQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnreadNotificationCountQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _unitOfWork.Repository<Notification>()
            .CountAsync(new UnreadNotificationsForUserSpecification(request.UserId));
        return Result.Success(count);
    }
}
