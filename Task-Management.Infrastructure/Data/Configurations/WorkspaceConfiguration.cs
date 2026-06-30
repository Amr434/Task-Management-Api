using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.Property(w => w.Name).IsRequired().HasMaxLength(100);
        builder.Property(w => w.Description).HasMaxLength(500);

        // Many-to-Many: Workspace <-> User
        builder.HasMany(w => w.Members)
               .WithMany(u => u.Workspaces)
               .UsingEntity(j => j.ToTable("WorkspaceMembers"));
    }
}
