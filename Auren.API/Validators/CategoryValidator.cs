using Auren.API.DTOs.Requests;
using FluentValidation;

namespace Auren.API.Validators
{
	public class CategoryValidator : AbstractValidator<CategoryRequest>
	{
		public CategoryValidator()
		{
			RuleFor(c => c.Name)
				.NotEmpty().WithMessage("Category name is required.")
				.MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
			
			RuleFor(c => c.TransactionType)
                .NotEmpty().WithMessage("Transaction type is required.")
                .IsInEnum().WithMessage("Invalid transaction type.");
        }
	}
}
