using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MythicNexus.Domain.Entities;

namespace MythicNexus.Infrastructure.Persistence.Configurations;

public class LoginAuditEventConfiguration : IEntityTypeConfiguration<LoginAuditEvent>
{
    public void Configure(EntityTypeBuilder<LoginAuditEvent> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.OccurredAt);
        builder.Property(e => e.EmailNormalized).HasMaxLength(320);
        builder.Property(e => e.FailureReason).HasMaxLength(120);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(512);
    }
}
