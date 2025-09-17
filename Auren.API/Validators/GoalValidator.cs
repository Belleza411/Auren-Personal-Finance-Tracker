using Auren.API.DTOs.Requests;
using FluentValidation;

namespace Auren.API.Validators
{
	public class GoalValidator : AbstractValidator<GoalRequest>
	{
		public GoalValidator()
		{
            RuleFor(g => g.Name)
                .NotEmpty().WithMessage("Goal name is required.")
                .MaximumLength(50).WithMessage("Goal name must not exceed 50 characters.");

            RuleFor(g => g.Description)
                .NotEmpty().WithMessage("Goal description is required.")
                .MaximumLength(255).WithMessage("Goal description must not exceed 255 characters.");

            RuleFor(g => g.Spent)
                .NotEmpty().WithMessage("Goal spent amount is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Goal spent amount cannot be negative.");

            RuleFor(g => g.Budget)
                .NotEmpty().WithMessage("Goal budget is required.")
                .GreaterThan(0).WithMessage("Goal budget must be greater than zero.");

            RuleFor(g => g.Status)
                .NotEmpty().WithMessage("Goal status is required.")
                .IsInEnum().WithMessage("Invalid goal status.");

            RuleFor(g => g.TargetDate)
                .NotEmpty().WithMessage("Goal target date is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Goal target date must be in the future.");
        }
	}
}
