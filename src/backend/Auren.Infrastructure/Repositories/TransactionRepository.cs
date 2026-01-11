 using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Specifications.Transactions;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;


namespace Auren.Infrastructure.Repositories
{
	public class TransactionRepository : Repository<Transaction>, ITransactionRepository
	{
		private readonly AurenDbContext _dbContext;
        private readonly ICategoryRepository _categoryRepository;

        public TransactionRepository(AurenDbContext dbContext, ICategoryRepository categoryRepository) : base(dbContext)
        {
			_dbContext = dbContext;
            _categoryRepository = categoryRepository;
		}

		public async Task<BalanceSummaryResponse> GetBalanceAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken)
		{
            var income = await _dbContext.Transactions
                .Where(t => t.UserId == userId &&
                            t.TransactionType == TransactionType.Income &&
                            t.CreatedAt >= start &&
                            t.CreatedAt <= end)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            var expense = await _dbContext.Transactions
                .Where(t => t.UserId == userId &&
                            t.TransactionType == TransactionType.Expense &&
                            t.CreatedAt >= start &&
                            t.CreatedAt <= end)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            return new BalanceSummaryResponse(
                Income: income,
                Expense: expense,
                Balance: income - expense
            );
        }

        public async Task<PagedResult<Transaction>> GetTransactionsAsync(
            Guid userId,
            TransactionFilter filter,
            int pageSize = 5, int pageNumber = 1,
            CancellationToken cancellationToken = default)
		{
            IEnumerable<Guid> categoryIds = [];

            if(filter.Category?.Any() == true)
            {
                categoryIds = await _categoryRepository.GetIdsByNamesAsync(userId, filter.Category, cancellationToken);
            }

            var skip = (pageNumber - 1) * pageSize;

            var spec = new TransactionFilterSpecification(userId, filter, categoryIds);
            var query = _dbContext.Transactions
                .Where(spec.ToExpression())
                .Include(t => t.Category);

            var totalCount = await query.CountAsync(cancellationToken);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
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

            var monthlyData = await _dbContext.Transactions
                .Where(t => t.UserId == userId && t.CreatedAt >= endDate)
                .GroupBy(t => new
                {
                    IsCurrentMonth = t.CreatedAt >= startDate,
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
    }
}