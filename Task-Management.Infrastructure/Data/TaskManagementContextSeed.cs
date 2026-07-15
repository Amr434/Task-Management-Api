using Task_Management.Domain.Entities;
using Task_Management.Domain.Enums;
using Microsoft.AspNetCore.Identity;
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

                var space = new Space
                {
                    Name = "First Space",
                    Description = "A seeded test space",
                    Color = "#7b68ee",
                    OwnerId = user.Id,
                    Members = new List<User> { user }
                };
                context.Spaces.Add(space);
                await context.SaveChangesAsync();

                var project = new Project
                {
                    Name = "First Project",
                    Description = "A seeded test project",
                    SpaceId = space.Id
                };
                context.Projects.Add(project);
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
                    ProjectId = project.Id,
                    Status = TaskStatusLevel.ToDo,
                    Assignees = new List<User> { user },
                    Tags = new List<Tag> { tag }
                };
                context.TaskItems.Add(task);
                await context.SaveChangesAsync();
            }

            var roster = new[]
            {
                new User { FirstName = "Amr", LastName = "Khaled", Email = "amr@example.com", ExternalId = "amr-external-id" },
                new User { FirstName = "Mohamed", LastName = "Saleh", Email = "mohamed@example.com", ExternalId = "mohamed-external-id" },
                new User { FirstName = "Abeer", LastName = "Mousa", Email = "abeer@example.com", ExternalId = "abeer-external-id" },
                new User { FirstName = "Nour", LastName = "Eldin", Email = "nour@example.com", ExternalId = "nour-external-id" },
                new User { FirstName = "Hossam", LastName = "Eldin", Email = "hossam@example.com", ExternalId = "hossam-external-id"},
                new User { FirstName = "Sara", LastName = "Ali", Email = "sara@example.com", ExternalId = "sara-external-id" },
                new User { FirstName = "Omar", LastName = "Hassan", Email = "omar@example.com", ExternalId = "omar-external-id" },
                new User { FirstName = "Lina", LastName = "Youssef", Email = "lina@example.com", ExternalId = "lina-external-id" },
            };

            var existingEmails = context.Users.Select(u => u.Email).ToList();
            var missing = roster.Where(u => !existingEmails.Contains(u.Email)).ToList();
            if (missing.Count > 0)
            {
                context.Users.AddRange(missing);
                await context.SaveChangesAsync();
            }

            var hasher = new PasswordHasher<User>();
            const string defaultPassword = "ChangeMe123!";

            var usersNeedingAuth = context.Users.Where(u => u.PasswordHash == "" || u.PasswordHash == null).ToList();
            foreach (var u in usersNeedingAuth)
            {
                u.PasswordHash = hasher.HashPassword(u, defaultPassword);
                u.MustChangePassword = true;
                u.IsActive = true;
            }

            var admin = context.Users.FirstOrDefault(u => u.Email == "amr@example.com");
            if (admin is not null && admin.Role != UserRole.Admin)
            {
                admin.Role = UserRole.Admin;
            }

            if (usersNeedingAuth.Count > 0 || context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            await SeedSharedDashboardSpaceAsync(context);
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<TaskManagementContextSeed>();
            logger.LogError(ex, "An error occurred during database seeding.");
        }
    }

    private static async Task SeedSharedDashboardSpaceAsync(TaskManagementDbContext context)
    {
        const string sharedSpaceName = "Team Operations";
        if (context.Spaces.Any(s => s.Name == sharedSpaceName))
            return;

        var owner = context.Users.FirstOrDefault(u => u.Email == "test@example.com")
            ?? context.Users.First();
        var amr = context.Users.First(u => u.Email == "amr@example.com");
        var mohamed = context.Users.First(u => u.Email == "mohamed@example.com");
        var abeer = context.Users.First(u => u.Email == "abeer@example.com");
        var nour = context.Users.First(u => u.Email == "nour@example.com");

        var sharedSpace = new Space
        {
            Name = sharedSpaceName,
            Description = "Shared space for dashboard analytics",
            Color = "#7b68ee",
            Icon = "📊",
            OwnerId = owner.Id,
            Members = new List<User> { owner, amr, mohamed, abeer, nour },
        };
        context.Spaces.Add(sharedSpace);
        await context.SaveChangesAsync();

        var project = new Project
        {
            Name = "Sprint Board",
            Description = "Tasks for dashboard charts",
            SpaceId = sharedSpace.Id,
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

        var tasks = new List<TaskItem>
        {
            MakeTask("Unassigned backlog item 1", TaskStatusLevel.ToDo, 1, project.Id, null),
            MakeTask("Unassigned backlog item 2", TaskStatusLevel.ToDo, 2, project.Id, null),
            MakeTask("Unassigned backlog item 3", TaskStatusLevel.ToDo, 3, project.Id, null),
            MakeTask("Mohamed assigned task", TaskStatusLevel.ToDo, 4, project.Id, mohamed),
            MakeTask("Abeer assigned task", TaskStatusLevel.ToDo, 5, project.Id, abeer),
            MakeTask("Mohamed in progress", TaskStatusLevel.InProgress, 6, project.Id, mohamed),
            MakeTask("Abeer in progress", TaskStatusLevel.InProgress, 7, project.Id, abeer),
            MakeTask("Amr in progress", TaskStatusLevel.InProgress, 8, project.Id, amr),
            MakeTask("Mohamed completed this week", TaskStatusLevel.Complete, 9, project.Id, mohamed, startOfWeek.AddDays(1)),
            MakeTask("Abeer completed this week", TaskStatusLevel.Complete, 10, project.Id, abeer, startOfWeek.AddDays(2)),
            MakeTask("Nour completed this week", TaskStatusLevel.Complete, 11, project.Id, nour, today),
            MakeTask("Old completed task", TaskStatusLevel.Complete, 12, project.Id, amr, today.AddDays(-14)),
        };

        // Bulk unassigned tasks to mirror ClickUp-style workload numbers.
        for (var i = 13; i <= 45; i++)
        {
            tasks.Add(MakeTask($"Unassigned item {i}", TaskStatusLevel.ToDo, i, project.Id, null));
        }

        for (var i = 46; i <= 160; i++)
        {
            var assignee = (i % 4) switch
            {
                0 => mohamed,
                1 => abeer,
                2 => amr,
                _ => nour,
            };
            tasks.Add(MakeTask($"Completed task {i}", TaskStatusLevel.Complete, i, project.Id, assignee, today.AddDays(-(i % 30))));
        }

        context.TaskItems.AddRange(tasks);
        await context.SaveChangesAsync();

        if (!context.Dashboards.Any(d => d.SpaceId == sharedSpace.Id))
        {
            var now = DateTime.UtcNow;
            context.Dashboards.Add(new Dashboard
            {
                Name = "Dashboard",
                SpaceId = sharedSpace.Id,
                OwnerId = owner.Id,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddHours(-2),
                LastViewedAt = now.AddHours(-3),
            });
            await context.SaveChangesAsync();
        }
    }

    private static TaskItem MakeTask(
        string title,
        TaskStatusLevel status,
        int order,
        int projectId,
        User? assignee,
        DateTime? dueDate = null)
    {
        var task = new TaskItem
        {
            Title = title,
            Priority = PriorityLevel.Medium,
            Order = order,
            ProjectId = projectId,
            Status = status,
            DueDate = dueDate,
        };

        if (assignee is not null)
            task.Assignees = new List<User> { assignee };

        return task;
    }
}
