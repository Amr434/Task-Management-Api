using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Description).HasMaxLength(500);

        // Owner relationship
        builder.HasOne(s => s.Owner)
               .WithMany()
               .HasForeignKey(s => s.OwnerId)
               .OnDelete(DeleteBehavior.SetNull);

        // Many-to-Many: Space <-> User
        builder.HasMany(s => s.Members)
               .WithMany(u => u.Spaces)
               .UsingEntity(j => j.ToTable("SpaceMembers"));
    }
}
