using MediatR;
using Task_Management.Application.Common.Interfaces;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Invitations;
using Task_Management.Domain.Specifications.Projects;
using Task_Management.Domain.Specifications.Spaces;

namespace Task_Management.Application.Features.Invitations.Commands;

public class RespondToInvitationCommand : IRequest<Result<InvitationDto>>
{
    public int UserId { get; set; }
    public int InvitationId { get; set; }
    public bool Accept { get; set; }

    public RespondToInvitationCommand(int userId, int invitationId, bool accept)
    {
        UserId = userId;
        InvitationId = invitationId;
        Accept = accept;
    }
}

public class RespondToInvitationCommandHandler : IRequestHandler<RespondToInvitationCommand, Result<InvitationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvitationNotifier _notifier;

    public RespondToInvitationCommandHandler(IUnitOfWork unitOfWork, IInvitationNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<Result<InvitationDto>> Handle(RespondToInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _unitOfWork.Repository<Invitation>()
            .GetEntityWithSpec(new InvitationByIdSpecification(request.InvitationId));

        if (invitation is null)
        {
            return Result.Failure<InvitationDto>(new Error("Invitation.NotFound", "Invitation not found."));
        }
        if (invitation.InviteeId != request.UserId)
        {
            // Only the addressee can respond; report NotFound to avoid leaking invites.
            return Result.Failure<InvitationDto>(new Error("Invitation.NotFound", "Invitation not found."));
        }
        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result.Failure<InvitationDto>(new Error("Invitation.AlreadyResponded", "This invitation was already responded to."));
        }

        if (request.Accept)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(request.UserId);
            if (user is null)
            {
                return Result.Failure<InvitationDto>(new Error("Invitation.UserNotFound", "User not found."));
            }

            if (invitation.TargetType == InvitationTargetType.Space && invitation.SpaceId.HasValue)
            {
                var space = await _unitOfWork.Repository<Space>()
                    .GetEntityWithSpec(new SpaceWithMembersSpecification(invitation.SpaceId.Value));
                if (space is null)
                {
                    return Result.Failure<InvitationDto>(new Error("Invitation.TargetGone", "The space no longer exists."));
                }
                if (!space.Members.Any(m => m.Id == user.Id))
                {
                    space.Members.Add(user);
                    _unitOfWork.Repository<Space>().Update(space);
                }
            }
            else if (invitation.TargetType == InvitationTargetType.Project && invitation.ProjectId.HasValue)
            {
                var project = await _unitOfWork.Repository<Project>()
                    .GetEntityWithSpec(new ProjectWithMembersSpecification(invitation.ProjectId.Value));
                if (project is null)
                {
                    return Result.Failure<InvitationDto>(new Error("Invitation.TargetGone", "The project no longer exists."));
                }
                if (!project.Members.Any(m => m.Id == user.Id))
                {
                    project.Members.Add(user);
                    _unitOfWork.Repository<Project>().Update(project);
                }
            }
        }

        invitation.Status = request.Accept ? InvitationStatus.Accepted : InvitationStatus.Declined;
        invitation.RespondedAtUtc = DateTime.UtcNow;
        _unitOfWork.Repository<Invitation>().Update(invitation);
        await _unitOfWork.CompleteAsync();

        var dto = new InvitationDto
        {
            Id = invitation.Id,
            TargetType = (int)invitation.TargetType,
            SpaceId = invitation.SpaceId,
            ProjectId = invitation.ProjectId,
            TargetName = invitation.Space?.Name ?? invitation.Project?.Name ?? string.Empty,
            InviterId = invitation.InviterId,
            InviteeId = invitation.InviteeId,
            Status = (int)invitation.Status,
            CreatedAtUtc = invitation.CreatedAtUtc
        };

        // Tell the inviter their invite was accepted/declined (best-effort).
        try
        {
            await _notifier.InvitationRespondedAsync(invitation.InviterId, dto);
        }
        catch
        {
            // Non-critical.
        }

        return Result.Success(dto);
    }
}
