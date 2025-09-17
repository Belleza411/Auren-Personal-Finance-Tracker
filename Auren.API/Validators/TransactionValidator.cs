using Auren.API.DTOs.Requests;
using FluentValidation;

namespace Auren.API.Validators
{
	public class TransactionValidator : AbstractValidator<TransactionRequest>
	{
		public TransactionValidator()
		{
			RuleFor(t => t.Name)
				.NotEmpty().WithMessage("Transaction name is required.")
				.MaximumLength(100).WithMessage("Transaction name must not exceed 100 characters.");

			RuleFor(t => t.Amount)
				.NotEmpty().WithMessage("Transaction amount is required.")
				.GreaterThan(0).WithMessage("Transaction amount must be greater than zero.");

            RuleFor(t => t.Category)
				.NotEmpty().WithMessage("Transaction category is required.");

			RuleFor(t => t.TransactionType)
				.NotEmpty().WithMessage("Transaction type is required.")
				.IsInEnum().WithMessage("Invalid Type");

            RuleFor(t => t.PaymentType)
                .NotEmpty().WithMessage("Transaction type is required.")
				.IsInEnum().WithMessage("Invalid Type");
        }
	}
}
