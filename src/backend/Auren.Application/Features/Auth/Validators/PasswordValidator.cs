using Auren.Application.Features.Auth.DTOs;
using FluentValidation;

namespace Auren.Application.Features.Auth.Validators
{
    public class PasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public PasswordValidator()
        {
            RuleFor(r => r.CurrentPassword)
                .NotEmpty().WithMessage("Current Password is required.")
                .Equal(r => r.NewPassword).WithMessage("Current Password and New Password cannot be the same.");   

            RuleFor(r => r.NewPassword)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one digit.")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

            RuleFor(r => r.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm Password is required.")
                .Equal(r => r.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
