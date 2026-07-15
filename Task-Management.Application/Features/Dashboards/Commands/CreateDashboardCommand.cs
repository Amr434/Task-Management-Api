using AutoMapper;
using FluentValidation;
using MediatR;
using Task_Management.Application.Features.Dashboards.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Spaces;

namespace Task_Management.Application.Features.Dashboards.Commands;

public class CreateDashboardCommand : IRequest<Result<DashboardDto>>
{
    public CreateDashboardDto Dto { get; set; }
    public int UserId { get; set; }

    public CreateDashboardCommand(CreateDashboardDto dto, int userId)
    {
        Dto = dto;
        UserId = userId;
    }
}

public class CreateDashboardCommandValidator : AbstractValidator<CreateDashboardCommand>
{
    public CreateDashboardCommandValidator()
    {
        RuleFor(v => v.Dto.SpaceId).GreaterThan(0);
    }
}

public class CreateDashboardCommandHandler : IRequestHandler<CreateDashboardCommand, Result<DashboardDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDashboardCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DashboardDto>> Handle(CreateDashboardCommand request, CancellationToken cancellationToken)
    {
        var space = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new AccessibleSpaceSpecification(request.Dto.SpaceId, request.UserId));

        if (space is null)
        {
            return Result.Failure<DashboardDto>(new Error("Space.NotFound", "Space not found or you don't have access to it."));
        }

        var now = DateTime.UtcNow;
        var dashboard = new Dashboard
        {
            Name = "Dashboard",
            SpaceId = space.Id,
            OwnerId = request.UserId,
            CreatedAt = now,
            UpdatedAt = now,
            LastViewedAt = now,
        };

        _unitOfWork.Repository<Dashboard>().Add(dashboard);
        await _unitOfWork.CompleteAsync();

        var created = await _unitOfWork.Repository<Dashboard>()
            .GetEntityWithSpec(new Task_Management.Domain.Specifications.Dashboards.DashboardByIdSpecification(dashboard.Id));

        var spaceWithMembers = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new SpaceWithMembersSpecification(dashboard.SpaceId));
        if (spaceWithMembers is not null && created is not null)
            created.Space = spaceWithMembers;

        return Result.Success(_mapper.Map<DashboardDto>(created!));
    }
}
