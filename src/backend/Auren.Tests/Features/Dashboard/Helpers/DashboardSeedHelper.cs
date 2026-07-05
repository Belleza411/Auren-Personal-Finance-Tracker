using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;

namespace Auren.Tests.Features.Dashboard.Helpers
{
    public static class DashboardSeedHelper
    {
        public static async Task SeedIncomeAsync(
            AurenDbContext db,
            Guid userId,
            Guid categoryId,
            decimal amount,
            DateTime date,
            CancellationToken ct = default)
        {
            var transaction = Fakers.TransactionFaker(userId)
                .RuleFor(t => t.TransactionType, TransactionType.Income)
                .RuleFor(t => t.CategoryId, categoryId)
                .RuleFor(t => t.Amount, amount)
                .RuleFor(t => t.TransactionDate, date)
                .Generate();

            await db.Transactions.AddAsync(transaction, ct);
            await db.SaveChangesAsync(ct);
        }

        public static async Task SeedExpenseAsync(
           AurenDbContext db,
           Guid userId,
           Guid categoryId,
           decimal amount,
           DateTime date,
           CancellationToken ct = default)
        {
            var transaction = Fakers.TransactionFaker(userId)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.CategoryId, categoryId)
                .RuleFor(t => t.Amount, amount)
                .RuleFor(t => t.TransactionDate, date)
                .Generate();

            await db.Transactions.AddAsync(transaction, ct);
            await db.SaveChangesAsync(ct);
        }

        public static async Task SeedIncomesAsync(
            AurenDbContext db,
            Guid userId,
            Guid categoryId,
            IEnumerable<(decimal Amount, DateTime Date)> entries,
            CancellationToken ct = default)
        {
            var transactions = entries.Select(e =>
                Fakers.TransactionFaker(userId)
                    .RuleFor(t => t.TransactionType, TransactionType.Income)
                    .RuleFor(t => t.CategoryId, categoryId)
                    .RuleFor(t => t.Amount, e.Amount)
                    .RuleFor(t => t.TransactionDate, e.Date)
                    .Generate()
            ).ToList();

            await db.Transactions.AddRangeAsync(transactions, ct);
            await db.SaveChangesAsync(ct);
        }

        public static async Task SeedExpensesAsync(
           AurenDbContext db,
           Guid userId,
           Guid categoryId,
           IEnumerable<(decimal Amount, DateTime Date)> entries,
           CancellationToken ct = default)
        {
            var transactions = entries.Select(e =>
                Fakers.TransactionFaker(userId)
                    .RuleFor(t => t.TransactionType, TransactionType.Expense)
                    .RuleFor(t => t.CategoryId, categoryId)
                    .RuleFor(t => t.Amount, e.Amount)
                    .RuleFor(t => t.TransactionDate, e.Date)
                    .Generate()
            ).ToList();

            await db.Transactions.AddRangeAsync(transactions, ct);
            await db.SaveChangesAsync(ct);
        }

        public static async Task<(Category Income, Category Expense)> SeedCategoriesAsync(
            AurenDbContext db,
            Guid userId,
            CancellationToken ct = default)
        {
            var income = Fakers.CategoryFaker(userId)
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate();

            var expense = Fakers.CategoryFaker(userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await db.Categories.AddRangeAsync([income, expense], ct);
            await db.SaveChangesAsync(ct);

            return (income, expense);
        }
    }
}
