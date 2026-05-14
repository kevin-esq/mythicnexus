using Microsoft.EntityFrameworkCore;
using MythicNexus.Domain.Entities;
using MythicNexus.Infrastructure.Persistence;

namespace MythicNexus.Infrastructure.Tests;

public sealed class MythicNexusDbContextTests
{
    [Fact]
    public async Task Can_add_and_query_user_with_in_memory_database()
    {
        await using var ctx = CreateContext();

        var id = Guid.NewGuid();
        ctx.Users.Add(
            new User
            {
                Id = id,
                Email = "u@example.com",
                Username = "u1",
                PasswordHash = "hash",
                CreatedAt = DateTimeOffset.UtcNow,
            });
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Users.AsNoTracking().SingleAsync(u => u.Id == id);

        Assert.Equal("u@example.com", loaded.Email);
        Assert.Equal("u1", loaded.Username);
    }

    private static MythicNexusDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MythicNexusDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MythicNexusDbContext(options);
    }
}
