using Microsoft.Extensions.Options;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;

namespace Task_Management.Infrastructure.Email;

public class EmailInvitationNotifier : IInvitationNotifier
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailQueue _queue;
    private readonly EmailSettings _settings;

    public EmailInvitationNotifier(IUnitOfWork unitOfWork, IEmailQueue queue, IOptions<EmailSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _queue = queue;
        _settings = settings.Value;
    }

    public async Task InvitationReceivedAsync(int inviteeUserId, InvitationDto invitation)
    {
        var invitee = (await EmailRecipients.LoadAsync(_unitOfWork, new[] { inviteeUserId })).FirstOrDefault();
        if (invitee is null) return;

        var inviter = string.IsNullOrWhiteSpace(invitation.InviterName) ? "Someone" : invitation.InviterName;
        var subject = $"{inviter} invited you to {TargetLabel(invitation)}";
        var html = EmailTemplates.Notification(
            subject,
            $"{inviter} invited you to join {TargetLabel(invitation)}. Open the app to accept or decline.",
            null, "View invitation", BaseUrl());

        _queue.Enqueue(new EmailMessage(invitee.Email, subject, html));
    }

    public async Task InvitationRespondedAsync(int inviterUserId, InvitationDto invitation)
    {
        var users = await EmailRecipients.LoadAsync(_unitOfWork, new[] { inviterUserId, invitation.InviteeId });
        var inviter = users.FirstOrDefault(u => u.Id == inviterUserId);
        if (inviter is null) return;

        var invitee = users.FirstOrDefault(u => u.Id == invitation.InviteeId);
        var inviteeName = invitee is null ? "Someone" : EmailRecipients.Name(invitee);
        var verb = invitation.Status == (int)InvitationStatus.Accepted ? "accepted" : "declined";

        var subject = $"{inviteeName} {verb} your invitation";
        var html = EmailTemplates.Notification(
            subject,
            $"{inviteeName} {verb} your invitation to {TargetLabel(invitation)}.",
            null, "Open Task Management", BaseUrl());

        _queue.Enqueue(new EmailMessage(inviter.Email, subject, html));
    }

    private static string TargetLabel(InvitationDto invitation)
    {
        var kind = invitation.TargetType == (int)InvitationTargetType.Space ? "space" : "project";
        return string.IsNullOrWhiteSpace(invitation.TargetName) ? $"a {kind}" : $"the {kind} \"{invitation.TargetName}\"";
    }

    private string BaseUrl() => _settings.AppBaseUrl.TrimEnd('/');
}
