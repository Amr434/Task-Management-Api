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
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.Project != null ? s.Project.Name : null))
            .ForMember(d => d.SpaceName, o => o.MapFrom(s => s.Project != null && s.Project.Space != null ? s.Project.Space.Name : null));
        CreateMap<CreateTaskDto, TaskItem>();

        // Tags
        CreateMap<Tag, Task_Management.Application.Features.Tags.DTOs.TagDto>();
        CreateMap<Task_Management.Application.Features.Tags.DTOs.CreateTagDto, Tag>();

        // Users
        CreateMap<User, Task_Management.Application.Features.Users.DTOs.UserDto>();

        // Comments
        CreateMap<Comment, Task_Management.Application.Features.Comments.DTOs.CommentDto>()
            .ForMember(d => d.Author, o => o.MapFrom(s => s.User))
            .ForMember(d => d.TaskTitle, o => o.MapFrom(s => s.TaskItem != null ? s.TaskItem.Title : null))
            .ForMember(d => d.ProjectId, o => o.MapFrom(s => s.TaskItem != null ? s.TaskItem.ProjectId : 0))
            .ForMember(d => d.ProjectName, o => o.MapFrom(s => s.TaskItem != null && s.TaskItem.Project != null ? s.TaskItem.Project.Name : null))
            .ForMember(d => d.SpaceName, o => o.MapFrom(s => s.TaskItem != null && s.TaskItem.Project != null && s.TaskItem.Project.Space != null ? s.TaskItem.Project.Space.Name : null));

        // Dashboards
        CreateMap<User, Task_Management.Application.Features.Dashboards.DTOs.DashboardMemberDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()))
            .ForMember(d => d.Initials, o => o.MapFrom(s => GetInitials($"{s.FirstName} {s.LastName}".Trim(), s.Email)));

        CreateMap<Dashboard, Task_Management.Application.Features.Dashboards.DTOs.DashboardDto>()
            .ForMember(d => d.SpaceName, o => o.MapFrom(s => s.Space != null ? s.Space.Name : string.Empty))
            .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner != null ? $"{s.Owner.FirstName} {s.Owner.LastName}".Trim() : string.Empty))
            .ForMember(d => d.OwnerInitials, o => o.MapFrom(s => GetInitials(s.Owner != null ? $"{s.Owner.FirstName} {s.Owner.LastName}".Trim() : string.Empty, s.Owner != null ? s.Owner.Email : null)))
            .ForMember(d => d.IsSpaceShared, o => o.MapFrom(s => s.Space != null && s.Space.Members != null && s.Space.Members.Count > 0))
            .ForMember(d => d.SharingMembers, o => o.MapFrom(s => s.Space != null && s.Space.Members != null ? s.Space.Members : new List<User>()));

        // Space & Project Members
        CreateMap<User, Task_Management.Application.Features.Spaces.DTOs.SpaceMemberDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()))
            .ForMember(d => d.Initials, o => o.MapFrom(s => GetInitials($"{s.FirstName} {s.LastName}".Trim(), s.Email)));

        CreateMap<User, Task_Management.Application.Features.Projects.DTOs.ProjectMemberDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()))
            .ForMember(d => d.Initials, o => o.MapFrom(s => GetInitials($"{s.FirstName} {s.LastName}".Trim(), s.Email)));
    }

    private static string GetInitials(string name, string? email)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
            return parts[0][0].ToString().ToUpperInvariant();
        }

        return string.IsNullOrWhiteSpace(email) ? "?" : email[0].ToString().ToUpperInvariant();
    }
}
