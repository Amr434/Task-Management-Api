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

        // One-to-Many: Space -> Projects
        builder.HasOne(p => p.Space)
               .WithMany(s => s.Projects)
               .HasForeignKey(p => p.SpaceId)
               .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many: Project <-> User (SharedProjects)
        builder.HasMany(p => p.Members)
               .WithMany(u => u.SharedProjects)
               .UsingEntity(j => j.ToTable("ProjectMembers"));
    }
}
