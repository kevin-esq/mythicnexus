using Microsoft.EntityFrameworkCore;
using MythicNexus.Api.Domain.Entities;

namespace MythicNexus.Api.Infrastructure.Persistence;

public class MythicNexusDbContext(DbContextOptions<MythicNexusDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<LoreEntry> LoreEntries => Set<LoreEntry>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedCampaigns)
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Characters)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoreEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.LoreEntries)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Tags)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CampaignId, e.Name }).IsUnique();
        });

        modelBuilder.Entity<LoreEntry>()
            .HasMany(e => e.Tags)
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
