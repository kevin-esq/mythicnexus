using FluentValidation;
using MythicNexus.Application.Users.DTOs;

namespace MythicNexus.Application.Users.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(512);
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(200)
            .Matches(@"[A-Z]")
            .Matches(@"[a-z]")
            .Matches(@"\d")
            .Matches(@"[\W_]");
    }
}
