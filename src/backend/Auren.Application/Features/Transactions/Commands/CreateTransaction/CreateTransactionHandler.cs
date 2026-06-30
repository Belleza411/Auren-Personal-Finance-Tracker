using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Transactions.DTOs;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Features.Transactions.Commands.CreateTransaction
{
    public class CreateTransactionHandler(
        IAppDbContext db,
        IValidator<TransactionDto> validator)
    {
        public async Task<Result<Transaction>> Handle(
            CreateTransactionCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return Result.Failure<Transaction>(Error.ValidationFailed(errors));
            }

            var category = await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.UserId == cmd.UserId &&
                    c.Name == cmd.Dto.Category,
                    ct);

            if (category == null)
                return Result.Failure<Transaction>(Error.NotFound("Category not found. "));

            if (cmd.Dto.TransactionType != category.TransactionType)
                return Result.Failure<Transaction>(Error.TypeMismatch("Transaction type does not match category type."));

            if(cmd.Dto.TransactionType == TransactionType.Expense)
            {
                var balance = await GetBalanceAsync(cmd.UserId, ct);
                if (balance < cmd.Dto.Amount)
                    return Result.Failure<Transaction>(
                        Error.NotEnoughBalance("Not enough balance."));
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = cmd.UserId,
                CategoryId = category.Id,
                TransactionType = category.TransactionType,
                Name = cmd.Dto.Name,
                Amount = cmd.Dto.Amount,
                PaymentType = cmd.Dto.PaymentType,
                CreatedAt = DateTime.UtcNow,
                TransactionDate = cmd.Dto.TransactionDate
            };

            await db.Transactions.AddAsync(transaction, ct);
            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(transaction)
                : Result.Failure<Transaction>(Error.CreateFailed("Failed to create transaction."));
        }

        private async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken ct)
        {
            var totals = await db.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.TransactionType)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
                .ToListAsync(ct);

            var income = totals.FirstOrDefault(x => x.Type == TransactionType.Income)?.Total ?? 0;
            var expense = totals.FirstOrDefault(x => x.Type == TransactionType.Expense)?.Total ?? 0;

            return income - expense;
        }
    }
}