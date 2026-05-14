using FluentValidation;
using MythicNexus.Application.Users.DTOs;

namespace MythicNexus.Application.Users.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("validation.email_required")
            .EmailAddress()
            .WithMessage("validation.email_invalid")
            .MaximumLength(320)
            .WithMessage("validation.email_max_length");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("validation.password_required")
            .MaximumLength(200)
            .WithMessage("validation.password_max_length");
    }
}
