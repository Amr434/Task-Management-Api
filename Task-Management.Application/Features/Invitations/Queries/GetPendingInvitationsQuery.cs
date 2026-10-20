using MediatR;
using Task_Management.Application.Features.Invitations.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Invitations;

namespace Task_Management.Application.Features.Invitations.Queries;

public class GetPendingInvitationsQuery : IRequest<Result<IEnumerable<InvitationDto>>>
{
    public int UserId { get; set; }

    public GetPendingInvitationsQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetPendingInvitationsQueryHandler : IRequestHandler<GetPendingInvitationsQuery, Result<IEnumerable<InvitationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingInvitationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<InvitationDto>>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _unitOfWork.Repository<Invitation>()
            .ListAsync(new PendingInvitationsForUserSpecification(request.UserId));

        var dtos = invitations.Select(i => new InvitationDto
        {
            Id = i.Id,
            TargetType = (int)i.TargetType,
            SpaceId = i.SpaceId,
            ProjectId = i.ProjectId,
            TargetName = i.Space?.Name ?? i.Project?.Name ?? string.Empty,
            InviterId = i.InviterId,
            InviterName = i.Inviter != null ? $"{i.Inviter.FirstName} {i.Inviter.LastName}".Trim() : string.Empty,
            InviteeId = i.InviteeId,
            Status = (int)i.Status,
            CreatedAtUtc = i.CreatedAtUtc
        });

        return Result.Success(dtos);
    }
}
