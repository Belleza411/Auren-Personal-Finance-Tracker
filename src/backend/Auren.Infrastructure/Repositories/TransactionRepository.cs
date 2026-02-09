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
                            t.TransactionDate >= start &&
                            t.TransactionDate <= end)
                .SumAsync(t => (decimal?)t.Amount, cancellationToken) ?? 0m;

            var expense = await _dbContext.Transactions
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

        public async Task<PagedResult<Transaction>> GetTransactionsAsync(
            Guid userId,
            TransactionFilter filter,
            int pageSize = 10, int pageNumber = 1,
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

    }
}