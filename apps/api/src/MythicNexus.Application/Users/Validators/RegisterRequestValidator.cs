using FluentValidation;
using MythicNexus.Application.Users.DTOs;

namespace MythicNexus.Application.Users.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("validation.email_required")
            .EmailAddress()
            .WithMessage("validation.email_invalid")
            .MaximumLength(320)
            .WithMessage("validation.email_max_length");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("validation.username_required")
            .MinimumLength(2)
            .WithMessage("validation.username_min_length")
            .MaximumLength(80)
            .WithMessage("validation.username_max_length")
            .Matches(@"^[\w\-.]+$")
            .WithMessage("validation.username_invalid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("validation.password_required")
            .MinimumLength(12)
            .WithMessage("validation.password_min_length")
            .MaximumLength(200)
            .WithMessage("validation.password_max_length")
            .Matches(@"[A-Z]")
            .WithMessage("validation.password_uppercase")
            .Matches(@"[a-z]")
            .WithMessage("validation.password_lowercase")
            .Matches(@"\d")
            .WithMessage("validation.password_digit")
            .Matches(@"[\W_]")
            .WithMessage("validation.password_special");
    }
}
