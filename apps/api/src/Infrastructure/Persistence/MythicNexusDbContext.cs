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
    public DbSet<LoreRelation> LoreRelations => Set<LoreRelation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MythicNexusDbContext).Assembly);
    }
}
