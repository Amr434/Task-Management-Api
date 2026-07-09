using AutoMapper;
using Task_Management.Application.Features.Projects.DTOs;
using Task_Management.Application.Features.Tasks.DTOs;
using Task_Management.Domain.Entities;

namespace Task_Management.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Spaces
        CreateMap<Space, Task_Management.Application.Features.Spaces.DTOs.SpaceDto>();
        CreateMap<Task_Management.Application.Features.Spaces.DTOs.CreateSpaceDto, Space>();

        // Projects
        CreateMap<Project, ProjectDto>();
        CreateMap<CreateProjectDto, Project>();


        // Tasks
        CreateMap<TaskItem, TaskItemDto>();
        CreateMap<CreateTaskDto, TaskItem>();

        // Tags
        CreateMap<Tag, Task_Management.Application.Features.Tags.DTOs.TagDto>();
        CreateMap<Task_Management.Application.Features.Tags.DTOs.CreateTagDto, Tag>();

        // Users
        CreateMap<User, Task_Management.Application.Features.Users.DTOs.UserDto>();
    }
}
