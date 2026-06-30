using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class ListConfiguration : IEntityTypeConfiguration<List>
{
    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.Property(l => l.Name).IsRequired().HasMaxLength(50);
        
        // One-to-Many: Project -> Lists
        builder.HasOne(l => l.Project)
               .WithMany(p => p.Lists)
               .HasForeignKey(l => l.ProjectId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
