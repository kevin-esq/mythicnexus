using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MythicNexus.Api.Domain.Entities;

namespace MythicNexus.Api.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.Username).IsUnique();
        builder.Property(e => e.Email).HasMaxLength(320);
        builder.Property(e => e.Username).HasMaxLength(80);
        builder.Property(e => e.PasswordHash).HasMaxLength(500);
    }
}
