using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Users.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Projects;
using Task_Management.Domain.Specifications.Spaces;

namespace Task_Management.Application.Features.Projects.Queries;

// Users who can work in a project (and thus be assigned to its tasks):
// the parent space's owner, accepted space members, and accepted
// project-level shares.
public class GetProjectMembersQuery : IRequest<Result<IEnumerable<UserDto>>>
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }

    public GetProjectMembersQuery(int projectId, int userId)
    {
        ProjectId = projectId;
        UserId = userId;
    }
}

public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProjectMembersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetProjectMembersQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Repository<Project>()
            .GetEntityWithSpec(new ProjectWithMembersSpecification(request.ProjectId));
        if (project is null)
        {
            return Result.Failure<IEnumerable<UserDto>>(new Error("Project.NotFound", "Project not found."));
        }

        var space = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new SpaceWithMembersSpecification(project.SpaceId));
        if (space is null)
        {
            return Result.Failure<IEnumerable<UserDto>>(new Error("Project.NotFound", "Project not found."));
        }

        // Same rule as AccessibleProjectSpecification, evaluated on the roster
        // we already loaded: only people in the project may list its members.
        var hasAccess = space.OwnerId == request.UserId
            || space.Members.Any(m => m.Id == request.UserId)
            || project.Members.Any(m => m.Id == request.UserId);
        if (!hasAccess)
        {
            return Result.Failure<IEnumerable<UserDto>>(new Error("Project.NotFound", "Project not found or you don't have access to it."));
        }

        var roster = new List<User>();
        if (space.OwnerId is int ownerId)
        {
            var owner = await _unitOfWork.Repository<User>().GetByIdAsync(ownerId);
            if (owner is not null) roster.Add(owner);
        }
        roster.AddRange(space.Members);
        roster.AddRange(project.Members);

        // Owner may also be a member row; space + project shares can overlap.
        var distinct = roster.GroupBy(u => u.Id).Select(g => g.First()).ToList();
        return Result.Success(_mapper.Map<IEnumerable<UserDto>>(distinct));
    }
}
