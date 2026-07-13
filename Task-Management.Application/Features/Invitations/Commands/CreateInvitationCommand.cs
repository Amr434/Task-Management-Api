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

public class CreateInvitationCommand : IRequest<Result<InvitationDto>>
{
    public int InviterId { get; set; }
    public CreateInvitationDto Dto { get; set; }

    public CreateInvitationCommand(int inviterId, CreateInvitationDto dto)
    {
        InviterId = inviterId;
        Dto = dto;
    }
}

public class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, Result<InvitationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInvitationNotifier _notifier;

    public CreateInvitationCommandHandler(IUnitOfWork unitOfWork, IInvitationNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<Result<InvitationDto>> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (!Enum.IsDefined(typeof(InvitationTargetType), dto.TargetType))
        {
            return Result.Failure<InvitationDto>(new Error("Invitation.InvalidTarget", "TargetType must be 0 (Space) or 1 (Project)."));
        }
        var targetType = (InvitationTargetType)dto.TargetType;

        if (dto.InviteeUserId == request.InviterId)
        {
            return Result.Failure<InvitationDto>(new Error("Invitation.SelfInvite", "You cannot invite yourself."));
        }

        var invitee = await _unitOfWork.Repository<User>().GetByIdAsync(dto.InviteeUserId);
        if (invitee is null)
        {
            return Result.Failure<InvitationDto>(new Error("Invitation.InviteeNotFound", "The invited user does not exist."));
        }

        var inviter = await _unitOfWork.Repository<User>().GetByIdAsync(request.InviterId);
        if (inviter is null)
        {
            return Result.Failure<InvitationDto>(new Error("Invitation.InviterNotFound", "Inviter not found."));
        }

        string targetName;
        int? spaceId = null;
        int? projectId = null;

        if (targetType == InvitationTargetType.Space)
        {
            // The inviter must have access to the space they are sharing.
            var space = await _unitOfWork.Repository<Space>()
                .GetEntityWithSpec(new AccessibleSpaceSpecification(dto.TargetId, request.InviterId));
            if (space is null)
            {
                return Result.Failure<InvitationDto>(new Error("Invitation.NoAccess", "Space not found or you don't have access to it."));
            }

            // Invitee already sees this space? Nothing to invite them to.
            var inviteeAccess = await _unitOfWork.Repository<Space>()
                .GetEntityWithSpec(new AccessibleSpaceSpecification(dto.TargetId, dto.InviteeUserId));
            if (inviteeAccess is not null)
            {
                return Result.Failure<InvitationDto>(new Error("Invitation.AlreadyMember", "This user already has access to the space."));
            }

            targetName = space.Name;
            spaceId = space.Id;
        }
        else
        {
            var project = await _unitOfWork.Repository<Project>()
                .GetEntityWithSpec(new AccessibleProjectSpecification(dto.TargetId, request.InviterId));
            if (project is null)
            {
                return Result.Failure<InvitationDto>(new Error("Invitation.NoAccess", "Project not found or you don't have access to it."));
            }

            var inviteeAccess = await _unitOfWork.Repository<Project>()
                .GetEntityWithSpec(new AccessibleProjectSpecification(dto.TargetId, dto.InviteeUserId));
            if (inviteeAccess is not null)
            {
                return Result.Failure<InvitationDto>(new Error("Invitation.AlreadyMember", "This user already has access to the project."));
            }

            targetName = project.Name;
            projectId = project.Id;
        }

        var duplicate = await _unitOfWork.Repository<Invitation>()
            .GetEntityWithSpec(new DuplicatePendingInvitationSpecification(targetType, dto.TargetId, dto.InviteeUserId));
        if (duplicate is not null)
        {
            return Result.Failure<InvitationDto>(new Error("Invitation.Duplicate", "This user already has a pending invitation."));
        }

        var invitation = new Invitation
        {
            TargetType = targetType,
            SpaceId = spaceId,
            ProjectId = projectId,
            InviterId = request.InviterId,
            InviteeId = dto.InviteeUserId,
            Status = InvitationStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        _unitOfWork.Repository<Invitation>().Add(invitation);
        await _unitOfWork.CompleteAsync();

        var result = new InvitationDto
        {
            Id = invitation.Id,
            TargetType = (int)targetType,
            SpaceId = spaceId,
            ProjectId = projectId,
            TargetName = targetName,
            InviterId = inviter.Id,
            InviterName = $"{inviter.FirstName} {inviter.LastName}".Trim(),
            InviteeId = dto.InviteeUserId,
            Status = (int)InvitationStatus.Pending,
            CreatedAtUtc = invitation.CreatedAtUtc
        };

        // Real-time nudge to the invitee; failure to deliver must not fail the invite.
        try
        {
            await _notifier.InvitationReceivedAsync(dto.InviteeUserId, result);
        }
        catch
        {
            // Invitee still sees it in their inbox on next fetch.
        }

        return Result.Success(result);
    }
}
