using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MythicNexus.Domain.Entities;

namespace MythicNexus.Infrastructure.Persistence.Configurations;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200);
        builder.Property(e => e.Race).HasMaxLength(100);
        builder.Property(e => e.Class).HasMaxLength(100);
        builder.HasOne(e => e.Campaign)
            .WithMany(c => c.Characters)
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.OwnerUserId);
        builder.HasIndex(e => new { e.CampaignId, e.DeletedAt });
        builder.Property(e => e.Level).HasDefaultValue(1);
        builder.HasOne(e => e.Owner)
            .WithMany(u => u.OwnedCharacters)
            .HasForeignKey(e => e.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
