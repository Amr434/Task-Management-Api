using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class CommentReadConfiguration : IEntityTypeConfiguration<CommentRead>
{
    public void Configure(EntityTypeBuilder<CommentRead> builder)
    {
        // One "read" row per user per comment.
        builder.HasIndex(r => new { r.CommentId, r.UserId }).IsUnique();

        // Deleting a comment removes its read rows.
        builder.HasOne(r => r.Comment)
            .WithMany(c => c.Reads)
            .HasForeignKey(r => r.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict (not cascade) to avoid SQL Server's "multiple cascade paths"
        // error: comments already cascade from users.
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
