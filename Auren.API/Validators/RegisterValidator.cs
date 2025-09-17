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
				.MinimumLength(4).WithMessage("Password must be at least 4 characters long.")
				.MaximumLength(8).WithMessage("Password must not exceed 8 characters.");

			RuleFor(r => r.ConfirmPassword)
				.NotEmpty().WithMessage("Confirm Password is required.")
                .MinimumLength(4).WithMessage("Password must be at least 4 characters long.")
                .MaximumLength(8).WithMessage("Password must not exceed 8 characters.")
                .Equal(r => r.Password).WithMessage("Passwords do not match.");

			RuleFor(r => r.FirstName)
				.NotEmpty().WithMessage("First Name is required.")
				.MinimumLength(2).WithMessage("First Name must be at least 2 characters long.")
				.MaximumLength(50).WithMessage("First Name must not exceed 50 characters.");

            RuleFor(r => r.LastName)
                .NotEmpty().WithMessage("Last Name is required.")
                .MinimumLength(2).WithMessage("Last Name must be at least 2 characters long.")
                .MaximumLength(50).WithMessage("Last Name must not exceed 50 characters.");
        }
	}
}
