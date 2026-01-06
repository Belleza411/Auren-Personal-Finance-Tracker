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
using System.Runtime.InteropServices;


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

		public async Task<BalanceSummaryResponse> GetBalanceAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken)
		{
            var income = await dbContext.Transactions
                .Where(t => t.UserId == userId &&
                            t.TransactionType == TransactionType.Income &&
                            t.TransactionDate >= start &&
                            t.TransactionDate <= end)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            var expense = await dbContext.Transactions
                .Where(t => t.UserId == userId &&
                            t.TransactionType == TransactionType.Expense &&
                            t.TransactionDate >= start &&
                            t.TransactionDate <= end)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            return new BalanceSummaryResponse(
                Income: income,
                Expense: expense,
                Balance: income - expense
            );
        }

		public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken) 
            => await dbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId, cancellationToken);

        public async Task<PagedResult<Transaction>> GetTransactionsAsync(
            Guid userId,
            TransactionFilter filter,
            int pageSize = 5, int pageNumber = 1,
            CancellationToken cancellationToken = default)
		{
            var skip = (pageNumber - 1) * pageSize;

            var query = dbContext.Transactions
                .Where(t => t.UserId == userId);

            if (HasActiveFilters(filter))
            {
                query = ApplyTransactionFilters(query, filter, userId);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PagedResult<Transaction>
            {
                Items = transactions,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
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

            var searchTerm = filter.SearchTerm?.Trim();

            if (string.IsNullOrEmpty(searchTerm))
                return query;

            var isAmount = decimal.TryParse(searchTerm, out var amount);
            var isDate = DateTime.TryParse(searchTerm, out var date);

            var isTransactionType = Enum.TryParse<TransactionType>(searchTerm, true, out var transactionType);
            var isPaymentType = Enum.TryParse<PaymentType>(searchTerm, true, out var paymentType);

            query = query.Where(t =>
                t.Name.Contains(searchTerm) ||
                (isTransactionType && t.TransactionType == transactionType) ||
                (isPaymentType && t.PaymentType == paymentType) ||
                (isAmount && t.Amount == amount) ||
                (isDate && t.TransactionDate.Date == date.Date) ||

                dbContext.Categories.Any(c =>
                    c.CategoryId == t.CategoryId &&
                    c.UserId == userId &&
                    c.Name.Contains(searchTerm)
                )
            );

            if (filter.TransactionType.HasValue)
                query = query.Where(t => t.TransactionType == filter.TransactionType.Value);

            if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                query = query.Where(t => t.TransactionDate >= filter.StartDate && t.TransactionDate <= filter.EndDate);
            
            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);
            
            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);          

            if (filter.Category != null && filter.Category.Any())
            {
                query = query.Where(t => dbContext.Categories
                    .Where(c => c.CategoryId == t.CategoryId
                        && filter.Category.Contains(c.Name)
                        && c.UserId == userId).Any());
            }

            if(filter.PaymentType.HasValue)
            {
                query = query.Where(t => t.PaymentType == filter.PaymentType.Value);
            }

            return query;
        }

        private static bool HasActiveFilters(TransactionFilter filter)
        {
            if (filter == null) return false;

            return !string.IsNullOrWhiteSpace(filter.SearchTerm) ||
                   filter.TransactionType.HasValue ||
                   filter.MinAmount.HasValue ||
                   filter.MaxAmount.HasValue ||
                   filter.StartDate.HasValue ||
                   filter.EndDate.HasValue ||
                   (filter.Category != null && filter.Category.Any()) ||
                   filter.PaymentType.HasValue;
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

            var currentBalance = await GetBalanceAsync(userId, startDate, endDate, cancellationToken);
            var lastMonthBalance = await GetBalanceAsync(userId, startDate, endDate, cancellationToken);

            var balanceChange = CalculatePercentageChange(currentBalance.Balance, lastMonthBalance.Balance, true);
            var incomeChange = CalculatePercentageChange(currentIncome, lastMonthIncome, false);
            var expenseChange = CalculatePercentageChange(currentExpense, lastMonthExpense, false);

            return new DashboardSummaryResponse(
                TotalBalance: new TransactionMetricResponse(
                    Amount: currentBalance.Balance,
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

