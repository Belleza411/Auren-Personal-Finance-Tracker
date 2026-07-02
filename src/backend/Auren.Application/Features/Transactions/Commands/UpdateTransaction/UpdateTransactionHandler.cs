using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Transactions.DTOs;
using Auren.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Features.Transactions.Commands.UpdateTransaction
{
    public class UpdateTransactionHandler(
        IAppDbContext db,
        IValidator<TransactionDto> validator)
    {
        public async Task<Result<Transaction>> Handle(
            UpdateTransactionCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Transaction>(Error.ValidationFailed(errors));
            }

            var transaction = await db.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Id == cmd.TransactionId &&
                    t.UserId == cmd.UserId,
                    ct);

            if (transaction == null)
                return Result.Failure<Transaction>(Error.NotFound("Transaction not found"));

            var category = await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.UserId == cmd.UserId &&
                    c.Name == cmd.Dto.Category, ct);

            if (category == null)
                return Result.Failure<Transaction>(Error.NotFound("Category not found. "));

            if(cmd.Dto.TransactionType != category.TransactionType)
                return Result.Failure<Transaction>(Error.TypeMismatch("Transaction type does not match category type."));

            transaction.Name = cmd.Dto.Name;
            transaction.Amount = cmd.Dto.Amount;
            transaction.PaymentType = cmd.Dto.PaymentType;
            transaction.TransactionType = cmd.Dto.TransactionType;
            transaction.CategoryId = category.Id;
            transaction.TransactionDate = cmd.Dto.TransactionDate;

            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(transaction)
                : Result.Failure<Transaction>(Error.UpdateFailed("Failed to update transaction."));
        }
    }
}
