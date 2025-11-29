using Auren.Application.DTOs.Requests;
using FluentValidation;

namespace Auren.Application.Validators
{
	public class LoginValidator : AbstractValidator<LoginRequest>
	{
		public LoginValidator()
		{
            RuleFor(l => l.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(l => l.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
        }
	}
}
