using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MythicNexus.Api.Domain.Entities;

namespace MythicNexus.Api.Infrastructure.Persistence.Configurations;

public class LoreRelationConfiguration : IEntityTypeConfiguration<LoreRelation>
{
    public void Configure(EntityTypeBuilder<LoreRelation> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RelationType).HasMaxLength(80);
        builder.HasIndex(e => new { e.CampaignId, e.SourceLoreEntryId, e.TargetLoreEntryId, e.RelationType }).IsUnique();

        builder.HasOne(e => e.Campaign)
            .WithMany(c => c.LoreRelations)
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Source)
            .WithMany(l => l.OutgoingRelations)
            .HasForeignKey(e => e.SourceLoreEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Target)
            .WithMany(l => l.IncomingRelations)
            .HasForeignKey(e => e.TargetLoreEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
