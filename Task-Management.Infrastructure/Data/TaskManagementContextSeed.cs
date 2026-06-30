using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Task_Management.Infrastructure.Data;

public class TaskManagementContextSeed
{
    public static async Task SeedAsync(TaskManagementDbContext context, ILoggerFactory loggerFactory)
    {
        try
        {
            if (!context.Users.Any())
            {
                var user = new User
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    ExternalId = "test-external-id"
                };
                context.Users.Add(user);
                await context.SaveChangesAsync();

                var workspace = new Workspace
                {
                    Name = "Default Workspace",
                    Description = "The default workspace for testing.",
                    Members = new List<User> { user }
                };
                context.Workspaces.Add(workspace);
                await context.SaveChangesAsync();

                var project = new Project
                {
                    Name = "First Project",
                    Description = "A seeded test project",
                    WorkspaceId = workspace.Id
                };
                context.Projects.Add(project);
                await context.SaveChangesAsync();

                var listToDo = new List { Name = "To Do", Order = 1, ProjectId = project.Id };
                var listInProgress = new List { Name = "In Progress", Order = 2, ProjectId = project.Id };
                var listDone = new List { Name = "Done", Order = 3, ProjectId = project.Id };
                
                context.Lists.AddRange(listToDo, listInProgress, listDone);
                await context.SaveChangesAsync();

                var tag = new Tag { Name = "Bug", ColorHex = "#FF0000" };
                context.Tags.Add(tag);
                await context.SaveChangesAsync();

                var task = new TaskItem
                {
                    Title = "Seed initial database",
                    Description = "Ensure the database has default data.",
                    Priority = PriorityLevel.High,
                    Order = 1,
                    ListId = listToDo.Id,
                    Assignees = new List<User> { user },
                    Tags = new List<Tag> { tag }
                };
                context.TaskItems.Add(task);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<TaskManagementContextSeed>();
            logger.LogError(ex, "An error occurred during database seeding.");
        }
    }
}
