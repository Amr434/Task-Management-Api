using AutoMapper;
using Task_Management.Application.Features.Lists.DTOs;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Application.Features.Workspaces.DTOs;
using Task_Management.Domain.Entities;

namespace Task_Management.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Workspaces
        CreateMap<Workspace, WorkspaceDto>();
        CreateMap<CreateWorkspaceDto, Workspace>();

        // Projects
        CreateMap<Project, ProjectDto>();
        CreateMap<CreateProjectDto, Project>();

        // Lists
        CreateMap<List, ListDto>();
        CreateMap<CreateListDto, List>();

        // Tasks
        CreateMap<TaskItem, TaskItemDto>();
        CreateMap<CreateTaskDto, TaskItem>();

        // Tags
        CreateMap<Tag, Task_Management.Application.Features.Tags.DTOs.TagDto>();
        CreateMap<Task_Management.Application.Features.Tags.DTOs.CreateTagDto, Tag>();
    }
}
