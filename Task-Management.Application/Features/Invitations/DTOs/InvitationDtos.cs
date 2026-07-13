namespace Task_Management.Application.Features.Invitations.DTOs;

public class CreateInvitationDto
{
    public int TargetType { get; set; } // InvitationTargetType: Space=0, Project=1
    public int TargetId { get; set; }
    public int InviteeUserId { get; set; }
}

public class InvitationDto
{
    public int Id { get; set; }
    public int TargetType { get; set; }   // Space=0, Project=1
    public int? SpaceId { get; set; }
    public int? ProjectId { get; set; }
    public string TargetName { get; set; } = string.Empty;
    public int InviterId { get; set; }
    public string InviterName { get; set; } = string.Empty;
    public int InviteeId { get; set; }
    public int Status { get; set; }        // Pending=0, Accepted=1, Declined=2
    public DateTime CreatedAtUtc { get; set; }
}
