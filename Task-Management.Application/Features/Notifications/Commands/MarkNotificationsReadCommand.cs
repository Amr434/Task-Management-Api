using MediatR;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Notifications;

namespace Task_Management.Application.Features.Notifications.Commands;

// Marks one of the current user's notifications as read (NotificationId set),
// or all of them ("Mark all as read"). Returns how many were newly marked.
public class MarkNotificationsReadCommand : IRequest<Result<int>>
{
    public int UserId { get; set; }
    public int? NotificationId { get; set; }

    public MarkNotificationsReadCommand(int userId, int? notificationId = null)
    {
        UserId = userId;
        NotificationId = notificationId;
    }
}

public class MarkNotificationsReadCommandHandler : IRequestHandler<MarkNotificationsReadCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(MarkNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var unread = await repo.ListAsync(new UnreadNotificationsForUserSpecification(request.UserId, request.NotificationId));
        if (unread.Count == 0) return Result.Success(0);

        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.ReadAtUtc = now;
            repo.Update(n);
        }
        await _unitOfWork.CompleteAsync();
        return Result.Success(unread.Count);
    }
}
