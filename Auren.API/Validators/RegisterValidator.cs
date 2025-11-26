using Auren.API.DTOs.Requests;
using FluentValidation;

namespace Auren.API.Validators
{
	public class RegisterValidator : AbstractValidator<RegisterRequest>
	{
		public RegisterValidator()
		{
			RuleFor(r => r.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Invalid email format.");

			RuleFor(r => r.Password)
				.NotEmpty().WithMessage("Password is required.")
				.MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
				.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
				.Matches(@"\d").WithMessage("Password must contain at least one digit.")
				.Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");

			RuleFor(r => r.ConfirmPassword)
				.NotEmpty().WithMessage("Confirm Password is required.")
                .Equal(r => r.Password).WithMessage("Passwords do not match.");

			RuleFor(r => r.FirstName)
				.NotEmpty().WithMessage("First Name is required.")
				.MaximumLength(50).WithMessage("First Name must not exceed 50 characters.");

            RuleFor(r => r.LastName)
                .NotEmpty().WithMessage("Last Name is required.")
                .MaximumLength(50).WithMessage("Last Name must not exceed 50 characters.");
        }
	}
}
