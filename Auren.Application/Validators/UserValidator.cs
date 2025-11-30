using Auren.Application.DTOs.Requests;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.Validators
{
    public class UserValidator : AbstractValidator<UserDto>
	{
		public UserValidator()
		{
			RuleFor(u => u.Email)
				.EmailAddress().WithMessage("Invalid email format.")
				.MaximumLength(255).WithMessage("Email must not exceed 255 characters.");
			RuleFor(u => u.FirstName)
				.MaximumLength(50).WithMessage("First name must not exceed 50 characters.");
			RuleFor(u => u.LastName)
				.MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");
            RuleFor(u => u.Currency)
				.Length(3).WithMessage("Currency must be a 3-letter ISO code.");
        }
	}
}
