using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.Property(a => a.FileName).IsRequired().HasMaxLength(255);
        builder.Property(a => a.FileUrl).IsRequired().HasMaxLength(1000);
        
        // One-to-Many: TaskItem -> Attachments
        builder.HasOne(a => a.TaskItem)
               .WithMany(t => t.Attachments)
               .HasForeignKey(a => a.TaskItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.UploadedBy)
               .WithMany()
               .HasForeignKey(a => a.UploadedById)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
