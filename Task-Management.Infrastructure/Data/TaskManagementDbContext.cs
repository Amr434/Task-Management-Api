using Microsoft.EntityFrameworkCore;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data;

public class TaskManagementDbContext : DbContext
{
    public DbSet<Space> Spaces { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
    }
}
