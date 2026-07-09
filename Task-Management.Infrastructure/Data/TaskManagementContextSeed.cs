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

                var space = new Space
                {
                    Name = "First Space",
                    Description = "A seeded test space",
                    Color = "#7b68ee",
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

            // Ensure a small roster of assignable users exists (idempotent by email),
            // so assignees can be picked even on an already-seeded database.
            var roster = new[]
            {
                new User { FirstName = "Amr", LastName = "Khaled", Email = "amr@example.com", ExternalId = "amr-external-id" },
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
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<TaskManagementContextSeed>();
            logger.LogError(ex, "An error occurred during database seeding.");
        }
    }
}
