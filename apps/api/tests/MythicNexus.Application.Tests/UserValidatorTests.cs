using MythicNexus.Application.Validation;
using MythicNexus.Domain.Entities;

namespace MythicNexus.Application.Tests;

public sealed class UserValidatorTests
{
    private readonly UserValidator _validator = new();

    [Fact]
    public void Valid_user_passes()
    {
        var user = new User
        {
            Email = "player@example.com",
            Username = "player_one",
            PasswordHash = "$2a$11$placeholderhashvaluehere",
        };

        var result = _validator.Validate(user);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var user = new User
        {
            Email = "not-an-email",
            Username = "ok",
            PasswordHash = "x",
        };

        var result = _validator.Validate(user);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(User.Email));
    }
}
