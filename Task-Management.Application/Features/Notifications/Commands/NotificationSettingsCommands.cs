using MediatR;
using Task_Management.Application.Features.Notifications.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;

namespace Task_Management.Application.Features.Notifications.Commands;

// The current user's email settings (which emails, instant or daily digest,
// and the daily/weekly summary).
public class GetNotificationSettingsQuery : IRequest<Result<NotificationSettingsDto>>
{
    public int UserId { get; set; }

    public GetNotificationSettingsQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetNotificationSettingsQueryHandler : IRequestHandler<GetNotificationSettingsQuery, Result<NotificationSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NotificationSettingsDto>> Handle(GetNotificationSettingsQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<NotificationSettingsDto>(new Error("User.NotFound", "User not found."));
        }
        return Result.Success(NotificationSettings.ToDto(user));
    }
}

public class UpdateNotificationSettingsCommand : IRequest<Result<NotificationSettingsDto>>
{
    public int UserId { get; set; }
    public NotificationSettingsDto Dto { get; set; }

    public UpdateNotificationSettingsCommand(int userId, NotificationSettingsDto dto)
    {
        UserId = userId;
        Dto = dto;
    }
}

public class UpdateNotificationSettingsCommandHandler : IRequestHandler<UpdateNotificationSettingsCommand, Result<NotificationSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateNotificationSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<NotificationSettingsDto>> Handle(UpdateNotificationSettingsCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (!Enum.IsDefined(typeof(EmailDeliveryMode), dto.EmailMode))
        {
            return Result.Failure<NotificationSettingsDto>(new Error("Notifications.InvalidMode", "EmailMode must be 0 (Instant), 1 (DailyDigest) or 2 (Off)."));
        }
        if (!Enum.IsDefined(typeof(SummaryFrequency), dto.EmailSummary))
        {
            return Result.Failure<NotificationSettingsDto>(new Error("Notifications.InvalidSummary", "EmailSummary must be 0 (Off), 1 (Daily) or 2 (Weekly)."));
        }

        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(request.UserId);
        if (user is null)
        {
            return Result.Failure<NotificationSettingsDto>(new Error("User.NotFound", "User not found."));
        }

        var newMode = (EmailDeliveryMode)dto.EmailMode;
        // Switching to the digest: it covers events from now on, not the ones
        // already emailed one by one.
        if (newMode == EmailDeliveryMode.DailyDigest && user.EmailMode != EmailDeliveryMode.DailyDigest)
        {
            user.LastDigestSentAtUtc = DateTime.UtcNow;
        }
        user.EmailMode = newMode;
        user.EmailSummary = (SummaryFrequency)dto.EmailSummary;
        user.MutedEmailCategories = NotificationSettings.MutedMask(dto);

        repo.Update(user);
        await _unitOfWork.CompleteAsync();
        return Result.Success(NotificationSettings.ToDto(user));
    }
}

internal static class NotificationSettings
{
    public static NotificationSettingsDto ToDto(User user) => new()
    {
        EmailMode = (int)user.EmailMode,
        EmailSummary = (int)user.EmailSummary,
        EmailAssignments = user.WantsEmail(EmailCategory.Assignments),
        EmailTaskUpdates = user.WantsEmail(EmailCategory.TaskUpdates),
        EmailComments = user.WantsEmail(EmailCategory.Comments),
        EmailMentions = user.WantsEmail(EmailCategory.Mentions),
        EmailInvitations = user.WantsEmail(EmailCategory.Invitations),
        EmailDueReminders = user.WantsEmail(EmailCategory.DueReminders),
    };

    public static int MutedMask(NotificationSettingsDto dto)
    {
        var mask = 0;
        void Mute(bool wanted, EmailCategory category)
        {
            if (!wanted) mask |= 1 << (int)category;
        }
        Mute(dto.EmailAssignments, EmailCategory.Assignments);
        Mute(dto.EmailTaskUpdates, EmailCategory.TaskUpdates);
        Mute(dto.EmailComments, EmailCategory.Comments);
        Mute(dto.EmailMentions, EmailCategory.Mentions);
        Mute(dto.EmailInvitations, EmailCategory.Invitations);
        Mute(dto.EmailDueReminders, EmailCategory.DueReminders);
        return mask;
    }
}
