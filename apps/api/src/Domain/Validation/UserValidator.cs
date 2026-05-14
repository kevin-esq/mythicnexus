using FluentValidation;
using MythicNexus.Api.Domain.Entities;

namespace MythicNexus.Api.Domain.Validation;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(u => u.Username)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(80)
            .Matches(@"^[\w\-.]+$")
            .WithMessage("Username may only contain letters, digits, underscore, hyphen, and period.");
        RuleFor(u => u.PasswordHash).NotEmpty().MaximumLength(500);
    }
}
