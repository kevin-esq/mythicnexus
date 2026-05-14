using FluentValidation;
using MythicNexus.Application.Users.DTOs;

namespace MythicNexus.Application.Users.Validators;

public sealed class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);
    }
}
