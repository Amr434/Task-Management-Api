using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;

namespace Task_Management.Domain.Specifications.Invitations;

// Pending invitations addressed to the user, with everything the inbox shows.
public class PendingInvitationsForUserSpecification : BaseSpecification<Invitation>
{
    public PendingInvitationsForUserSpecification(int userId)
        : base(i => i.InviteeId == userId && i.Status == InvitationStatus.Pending)
    {
        AddInclude(i => i.Inviter);
        AddInclude(i => i.Space!);
        AddInclude(i => i.Project!);
        AddOrderByDescending(i => i.CreatedAtUtc);
    }
}

public class InvitationByIdSpecification : BaseSpecification<Invitation>
{
    public InvitationByIdSpecification(int id) : base(i => i.Id == id)
    {
        AddInclude(i => i.Space!);
        AddInclude(i => i.Project!);
    }
}

// Guards against spamming the same person with the same pending invite.
public class DuplicatePendingInvitationSpecification : BaseSpecification<Invitation>
{
    public DuplicatePendingInvitationSpecification(InvitationTargetType targetType, int targetId, int inviteeId)
        : base(i => i.Status == InvitationStatus.Pending
            && i.InviteeId == inviteeId
            && i.TargetType == targetType
            && (targetType == InvitationTargetType.Space ? i.SpaceId == targetId : i.ProjectId == targetId))
    {
    }
}
