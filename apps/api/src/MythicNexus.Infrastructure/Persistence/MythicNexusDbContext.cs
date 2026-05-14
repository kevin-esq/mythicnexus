using Microsoft.EntityFrameworkCore;
using MythicNexus.Domain.Entities;

namespace MythicNexus.Infrastructure.Persistence;

public class MythicNexusDbContext(DbContextOptions<MythicNexusDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<LoginAuditEvent> LoginAuditEvents => Set<LoginAuditEvent>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<CampaignMember> CampaignMembers => Set<CampaignMember>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<LoreEntry> LoreEntries => Set<LoreEntry>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<LoreRelation> LoreRelations => Set<LoreRelation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MythicNexusDbContext).Assembly);
    }
}
