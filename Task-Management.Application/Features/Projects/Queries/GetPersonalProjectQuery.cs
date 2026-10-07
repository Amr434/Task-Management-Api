using AutoMapper;
using MediatR;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Domain.Entities;
using Task_Management.Domain.Interfaces;
using Task_Management.Domain.Shared;
using Task_Management.Domain.Specifications.Spaces;

namespace Task_Management.Application.Features.Projects.Queries;

// Resolves the current user's Personal List project, creating the hidden
// personal space + project on first use (ClickUp-style lazy creation).
public class GetPersonalProjectQuery : IRequest<Result<ProjectDto>>
{
    public int UserId { get; set; }

    public GetPersonalProjectQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetPersonalProjectQueryHandler : IRequestHandler<GetPersonalProjectQuery, Result<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPersonalProjectQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDto>> Handle(GetPersonalProjectQuery request, CancellationToken cancellationToken)
    {
        var space = await _unitOfWork.Repository<Space>()
            .GetEntityWithSpec(new PersonalSpaceForUserSpecification(request.UserId));

        if (space is null)
        {
            space = new Space
            {
                Name = "Personal List",
                Description = "Your private list",
                IsPersonal = true,
                OwnerId = request.UserId,
                Color = "#5b5b5b",
            };
            _unitOfWork.Repository<Space>().Add(space);
        }

        var project = space.Projects.FirstOrDefault();
        if (project is null)
        {
            project = new Project
            {
                Name = "Personal List",
                Space = space,
            };
            _unitOfWork.Repository<Project>().Add(project);
            await _unitOfWork.CompleteAsync();
        }

        var dto = _mapper.Map<ProjectDto>(project);
        return Result.Success(dto);
    }
}
