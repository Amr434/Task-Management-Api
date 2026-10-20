using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class TaskActivityConfiguration : IEntityTypeConfiguration<TaskActivity>
{
    public void Configure(EntityTypeBuilder<TaskActivity> builder)
    {
        builder.Property(a => a.OldValue).HasMaxLength(500);
        builder.Property(a => a.NewValue).HasMaxLength(500);

        // Stored as UTC; SQL Server drops the kind, so mark it on the way out.
        // Otherwise it serialises without a "Z" and clients read it as local time.
        builder.Property(a => a.CreatedAt)
               .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // History goes with its task.
        builder.HasOne(a => a.TaskItem)
               .WithMany(t => t.Activities)
               .HasForeignKey(a => a.TaskItemId)
               .OnDelete(DeleteBehavior.Cascade);

        // Restrict like Comment.UserId — a second cascade path to Users is not allowed.
        builder.HasOne(a => a.User)
               .WithMany()
               .HasForeignKey(a => a.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.TaskItemId, a.CreatedAt });
    }
}
