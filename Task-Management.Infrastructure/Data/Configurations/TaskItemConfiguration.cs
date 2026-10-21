using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasColumnType("nvarchar(max)");
        builder.Property(t => t.Priority).HasConversion<string>(); // Save enum as string

        builder.Property(t => t.Status).HasConversion<string>(); // Save enum as string

        // One-to-Many: Project -> Tasks
        builder.HasOne(t => t.Project)
               .WithMany(p => p.Tasks)
               .HasForeignKey(t => t.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);

        // Creator. Restrict like TaskActivity.UserId — a second cascade path to Users is not allowed.
        builder.HasOne(t => t.CreatedBy)
               .WithMany()
               .HasForeignKey(t => t.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);

        // Self-Referencing: ParentTask -> SubTasks
        builder.HasOne(t => t.ParentTask)
               .WithMany(t => t.SubTasks)
               .HasForeignKey(t => t.ParentTaskId)
               .OnDelete(DeleteBehavior.Restrict);

        // Many-to-Many: TaskItem <-> User (Assignees)
        builder.HasMany(t => t.Assignees)
               .WithMany(u => u.AssignedTasks)
               .UsingEntity(j => j.ToTable("TaskAssignees"));

        // Many-to-Many: TaskItem <-> Tag
        builder.HasMany(t => t.Tags)
               .WithMany(tag => tag.Tasks)
               .UsingEntity(j => j.ToTable("TaskTags"));
    }
}
