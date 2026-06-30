using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.Text).IsRequired().HasMaxLength(2000);
        
        // One-to-Many: TaskItem -> Comments
        builder.HasOne(c => c.TaskItem)
               .WithMany(t => t.Comments)
               .HasForeignKey(c => c.TaskItemId)
               .OnDelete(DeleteBehavior.Cascade);

        // One-to-Many: User -> Comments
        builder.HasOne(c => c.User)
               .WithMany() // No need for inverse navigation property on User right now
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
