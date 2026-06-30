using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);

        // One-to-Many: Workspace -> Projects
        builder.HasOne(p => p.Workspace)
               .WithMany(w => w.Projects)
               .HasForeignKey(p => p.WorkspaceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
