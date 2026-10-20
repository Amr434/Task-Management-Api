using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Payload).IsRequired();

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // The bell lists a user's newest first and counts the unread ones.
        builder.HasIndex(n => new { n.UserId, n.CreatedAtUtc });
        builder.HasIndex(n => new { n.UserId, n.ReadAtUtc });
    }
}
