using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task_Management.Domain.Entities;

namespace Task_Management.Infrastructure.Data.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.HasOne(i => i.Inviter)
               .WithMany()
               .HasForeignKey(i => i.InviterId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Invitee)
               .WithMany()
               .HasForeignKey(i => i.InviteeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Space)
               .WithMany()
               .HasForeignKey(i => i.SpaceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Project)
               .WithMany()
               .HasForeignKey(i => i.ProjectId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
