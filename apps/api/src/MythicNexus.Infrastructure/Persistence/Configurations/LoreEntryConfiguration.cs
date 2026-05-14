using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MythicNexus.Domain.Entities;

namespace MythicNexus.Infrastructure.Persistence.Configurations;

public class LoreEntryConfiguration : IEntityTypeConfiguration<LoreEntry>
{
    public void Configure(EntityTypeBuilder<LoreEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(300);
        builder.Property(e => e.Slug).HasMaxLength(200);
        builder.Property(e => e.Summary).HasMaxLength(2000);
        builder.HasIndex(e => new { e.CampaignId, e.Slug }).IsUnique();

        builder.HasOne(e => e.Campaign)
            .WithMany(c => c.LoreEntries)
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.CreatedBy)
            .WithMany(u => u.AuthoredLoreEntries)
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Tags)
            .WithMany(t => t.LoreEntries)
            .UsingEntity<Dictionary<string, object>>(
                "LoreEntryTags",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<LoreEntry>().WithMany().HasForeignKey("LoreEntryId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("LoreEntryId", "TagId");
                    j.ToTable("LoreEntryTags");
                });
    }
}
