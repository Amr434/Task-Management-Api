using Task_Management.Domain.Enums;

namespace Task_Management.Domain.Entities;

// A share request at either level: membership is granted only when the
// invitee accepts (space -> SpaceMembers, project -> ProjectMembers).
public class Invitation : BaseEntity
{
    public InvitationTargetType TargetType { get; set; }

    // Exactly one of these is set, matching TargetType.
    public int? SpaceId { get; set; }
    public Space? Space { get; set; }
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int InviterId { get; set; }
    public User Inviter { get; set; } = null!;
    public int InviteeId { get; set; }
    public User Invitee { get; set; } = null!;

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RespondedAtUtc { get; set; }
}
