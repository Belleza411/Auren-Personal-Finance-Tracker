using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Auren.Infrastructure.Repositories
{
	public class TransactionRepository(AurenDbContext dbContext) : ITransactionRepository
	{
		public async Task<Transaction> CreateTransactionAsync(Transaction transaction, Guid userId, CancellationToken cancellationToken)
		{
            await dbContext.Transactions.AddAsync(transaction, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            return transaction;     
        }

		public async Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
		{
            var transaction = await GetTransactionByIdAsync(transactionId, userId, cancellationToken);

            if (transaction == null) return false;

            dbContext.Transactions.Remove(transaction);
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

		public async Task<decimal> GetBalanceAsync(Guid userId, BalancePeriod balancePeriod, CancellationToken cancellationToken)
		{
            var currentMonthRange = DateTime.UtcNow.GetCurrentMonthRange();
            var lastMonthRange = DateTime.UtcNow.GetLastMonthRange();

            var query = dbContext.Transactions
                .Where(t => t.UserId == userId);

            switch (balancePeriod)
            {
                case BalancePeriod.CurrentMonth:
                    query = query.Where(t =>
                        t.TransactionDate >= currentMonthRange.start &&
                        t.TransactionDate <= currentMonthRange.end);
                    break;

                case BalancePeriod.LastMonth:
                    query = query.Where(t =>
                        t.TransactionDate >= lastMonthRange.start &&
                        t.TransactionDate <= lastMonthRange.end);
                    break;

                case BalancePeriod.AllTime:
                default:
                    break;
            }

            var grouped = await query
                .GroupBy(t => t.TransactionType)
                .Select(g => new
                {
                    Type = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync(cancellationToken);

            var income = grouped.FirstOrDefault(r => r.Type == TransactionType.Income)?.Total ?? 0;
            var expense = grouped.FirstOrDefault(r => r.Type == TransactionType.Expense)?.Total ?? 0;

            return income - expense;
        }

		public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken) 
            => await dbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId, cancellationToken);

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(
            Guid userId,
            TransactionFilter filter,
            int pageSize = 5, int pageNumber = 1,
            CancellationToken cancellationToken = default)
		{
            var skip = (pageNumber - 1) * pageSize;

            var query = dbContext.Transactions
                .Where(t => t.UserId == userId);

            if(HasActiveFilters(filter))
            {
                query = ApplyTransactionFilters(query, filter, userId);
            }

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return transactions;
        }

		public async Task<Transaction?> UpdateTransactionAsync(Guid transactionId, Guid userId, Transaction transaction, CancellationToken cancellationToken)
		{
            dbContext.Transactions.Update(transaction);
            await dbContext.SaveChangesAsync(cancellationToken);

            return transaction;
		}

        private IQueryable<Transaction> ApplyTransactionFilters(IQueryable<Transaction> query, TransactionFilter filter, Guid userId)
        {
            if (filter == null) return query;

            if(filter.IsIncome == true)
                query = query.Where(t => t.TransactionType == TransactionType.Income);
            
            if (filter.IsExpense == true)
                query = query.Where(t => t.TransactionType == TransactionType.Expense);

            if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                query = query.Where(t => t.TransactionDate >= filter.StartDate && t.TransactionDate <= filter.EndDate);
            
            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);
            
            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);          

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(t => dbContext.Categories
                    .Where(c => c.CategoryId == t.CategoryId
                    && c.Name.Contains(filter.Category)
                    && c.UserId == userId).Any()
                );
            }

            if(!string.IsNullOrEmpty(filter.PaymentMethod))
            {
                if(Enum.TryParse<PaymentType>(filter.PaymentMethod, true, out var paymentType))
                {
                    query = query.Where(t => t.PaymentType == paymentType);
                }
            }

            return query;
        }

        private static bool HasActiveFilters(TransactionFilter filter)
        {
            if (filter == null) return false;

            return filter.IsIncome.HasValue ||
                   filter.IsExpense.HasValue ||
                   filter.MinAmount.HasValue ||
                   filter.MaxAmount.HasValue ||
                   filter.StartDate.HasValue ||
                   filter.EndDate.HasValue ||
                   !string.IsNullOrEmpty(filter.Category) ||
                   !string.IsNullOrEmpty(filter.PaymentMethod);
        }

        public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid userId, TimePeriod? timePeriod, CancellationToken cancellationToken)
        {
            var (startDate, endDate) = timePeriod switch
            {
                TimePeriod.Last3Months => DateTime.Today.GetLast3MonthRange(),
                TimePeriod.Last6Months => DateTime.Today.GetLast6MonthRange(),
                TimePeriod.ThisYear => DateTime.Today.GetThisYearRange(),
                TimePeriod.LastMonth => DateTime.Today.GetLastMonthRange(),
                TimePeriod.ThisMonth => DateTime.Today.GetCurrentMonthRange(),
                TimePeriod.AllTime => (DateTime.MinValue, DateTime.Today),
                _ => (DateTime.MinValue, DateTime.Today) 
            };


            var monthlyData = await dbContext.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= endDate)
                .GroupBy(t => new
                {
                    IsCurrentMonth = t.TransactionDate >= startDate,
                    t.TransactionType
                })
                .Select(g => new
                {
                    g.Key.IsCurrentMonth,
                    g.Key.TransactionType,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync(cancellationToken);

            var currentIncome = monthlyData
                .FirstOrDefault(x => x.IsCurrentMonth && x.TransactionType == TransactionType.Income)?.Total ?? 0;

            var lastMonthIncome = monthlyData
                .FirstOrDefault(x => !x.IsCurrentMonth && x.TransactionType == TransactionType.Income)?.Total ?? 0;

            var currentExpense = monthlyData
                .FirstOrDefault(x => x.IsCurrentMonth && x.TransactionType == TransactionType.Expense)?.Total ?? 0;

            var lastMonthExpense = monthlyData
                .FirstOrDefault(x => !x.IsCurrentMonth && x.TransactionType == TransactionType.Expense)?.Total ?? 0;   

            var currentBalance = await GetBalanceAsync(userId, BalancePeriod.AllTime, cancellationToken);
            var lastMonthBalance = await GetBalanceAsync(userId, BalancePeriod.LastMonth, cancellationToken);

            var balanceChange = CalculatePercentageChange(currentBalance, lastMonthBalance, true);
            var incomeChange = CalculatePercentageChange(currentIncome, lastMonthIncome, false);
            var expenseChange = CalculatePercentageChange(currentExpense, lastMonthExpense, false);

            return new DashboardSummaryResponse(
                TotalBalance: new TransactionMetricResponse(
                    Amount: currentBalance,
                    PercentageChange: balanceChange
                ),
                Income: new TransactionMetricResponse(
                    Amount: currentIncome,
                    PercentageChange: incomeChange
                ),
                Expense: new TransactionMetricResponse(
                    Amount: currentExpense,
                    PercentageChange: expenseChange
                )
            );
        }

        private static decimal CalculatePercentageChange(decimal current, decimal previous, bool isTotalBalance)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            var change = isTotalBalance
                ? ((current - previous) / Math.Abs(previous)) * 100
                : ((current - previous) / previous) * 100;

            return Math.Round(change, 2);
        }

        public async Task<IEnumerable<Transaction>> GetExpensesAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken)
        {
            var expenses = await dbContext.Transactions
                .Where(t => t.UserId == userId 
                    && t.TransactionDate >= start 
                    && t.TransactionDate <= end)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return expenses;
        }
    }
}

