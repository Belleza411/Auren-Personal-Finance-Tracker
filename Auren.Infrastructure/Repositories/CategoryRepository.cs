using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Dapper;
using Microsoft.Data.SqlClient;
using Auren.Application.DTOs.Responses.Category;

namespace Auren.Infrastructure.Repositories
{
	public class CategoryRepository : ICategoryRepository
	{
        private readonly ILogger<CategoryRepository> _logger;
        private readonly AurenDbContext _dbContext;
        private readonly string _connectionString;

        public CategoryRepository(ILogger<CategoryRepository> logger, AurenDbContext dbContext, IConfiguration configuration)
		{
			_logger = logger;
			_dbContext = dbContext;
            _connectionString = configuration.GetConnectionString("AurenDbConnection") ?? throw new ArgumentNullException("Connection string not found.");
        }

		public async Task<Category> CreateCategoryAsync(Category category, Guid userId, CancellationToken cancellationToken)
		{
            await _dbContext.Categories.AddAsync(category, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return category;
        }

		public async Task<bool> DeleteCategoryAsync(Category category, Guid userId, CancellationToken cancellationToken)
		{
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Category with id of {CategoryId} was successfully deleted for {UserId}", category.CategoryId, userId);

            return true;
        }

		public async Task<IEnumerable<Category>> GetCategoriesAsync(Guid userId, CategoriesFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default)
		{
            var skip = (pageNumber - 1) * pageSize;

            var query = _dbContext.Categories
                .Where(c => c.UserId == userId);

            if (HasActiveFilter(filter))
            {
                var filteredQuery = ApplyFilters(query, filter);

                if (filteredQuery is IQueryable<Category> categoryQuery)
                {
                    query = categoryQuery;
                }
            }

            var categories = await query
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Retrieved {Count} categories for user {UserId}", categories.Count, userId);

            return categories;           
		}

		public async Task<Category?> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
            => await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId, cancellationToken);

		public async Task<Category?> UpdateCategoryAsync(Guid userId, Category category, CancellationToken cancellationToken)
		{
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync(cancellationToken);
			_logger.LogInformation("Category with id of {CategoryId} for {UserId} was successfull updated. ", category.CategoryId, userId);

			return category;			
        }

        public async Task<List<Category>> SeedDefaultCategoryToUserAsync(List<Category> categories, Guid userId, CancellationToken cancellationToken)
        {
            _dbContext.Categories.AddRange(categories);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return categories;
        }

        private IQueryable<object> ApplyFilters(IQueryable<Category> query, CategoriesFilter filter)
        {
            if (filter == null) return query;

            if (filter.IsIncome == true)
                query = query.Where(t => t.TransactionType == TransactionType.Income);

            if (filter.IsExpense == true)
                query = query.Where(t => t.TransactionType == TransactionType.Expense);

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(c => c.Name.Contains(filter.Category));
            }

            if(filter.Transactions > 0)
            {
                var q = from c in query
                        join t in _dbContext.Transactions on c.CategoryId equals t.CategoryId
                        group t by new { c.CategoryId, c.Name } into g
                        select new
                        {
                            CategoryName = g.Key.Name,
                            TransactionCount = g.Count()
                        };

                if (filter.Transactions > 0)
                {
                    q = q.Where(x => x.TransactionCount >= filter.Transactions);
                }

                return q.OrderByDescending(x => x.TransactionCount);
            }
            return query;
        }

        private bool HasActiveFilter(CategoriesFilter filter)
        {
            if (filter == null) return false;

            return filter.IsExpense.HasValue ||
                   filter.IsIncome.HasValue ||
                   !string.IsNullOrWhiteSpace(filter.Category) ||
                   filter.Transactions > 0;
        }

		public async Task<Category?> GetCategoryByNameAsync(Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken)
            =>  await _dbContext.Categories.FirstOrDefaultAsync(c => c.UserId == userId
                    && c.Name.ToLower() == categoryDto.Name.ToLower()
                    && c.TransactionType == categoryDto.TransactionType,
                    cancellationToken);

        public async Task<IEnumerable<CategoryOverviewResponse>> GetCategoryOverviewAsync(
            Guid userId,
            CategoryOverviewFilter filter,
            int pageSize = 5,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var offset = (pageNumber - 1) * pageSize;
            var sql = @"
                SELECT 
                    c.Name AS Category,
                    c.TransactionType,
                    ISNULL(SUM(t.Amount), 0) AS TotalSpending,
                    ROUND(ISNULL(AVG(t.Amount), 0), 2) AS AverageSpending,
                    ISNULL(COUNT(t.TransactionId), 0) AS TransactionCount,
                    MAX(t.TransactionDate) AS LastUsed
                FROM Categories c
                LEFT JOIN Transactions t 
                    ON c.CategoryId = t.CategoryId
                    AND t.UserId = @UserId
                    AND t.TransactionType = 2
                    AND (@MinAmount IS NULL OR t.Amount >= @MinAmount)
                    AND (@MaxAmount IS NULL OR t.Amount <= @MaxAmount)
                WHERE c.TransactionType = 2
                    AND (@Category IS NULL OR c.Name LIKE '%' + @Category + '%')
                GROUP BY c.Name, c.TransactionType
                HAVING 
                    (@MinTransactionCount IS NULL OR COUNT(t.TransactionId) >= @MinTransactionCount)
                    AND (@MaxTransactionCount IS NULL OR COUNT(t.TransactionId) <= @MaxTransactionCount)
                ORDER BY TransactionCount DESC, AverageSpending DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            ";

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var result = await connection.QueryAsync<CategoryOverviewResponse>(sql, new
            {
                UserId = userId,
				filter.Category,
                filter.MinAmount,
                filter.MaxAmount,
                filter.MinTransactionCount,
                filter.MaxTransactionCount,
                Offset = offset,
                pageSize,
            });

            return result;
        }

        public async Task<CategorySummaryResponse> GetCategoriesSummaryAsync(Guid userId, CancellationToken cancellationToken)
        {
            var summary = await _dbContext.Categories
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.Name,
                    TotalCategories = _dbContext.Transactions.Count(t => t.CategoryId == c.CategoryId),
                    HighestSpending = _dbContext.Transactions
                        .Where(t => t.CategoryId == c.CategoryId && t.TransactionType == TransactionType.Expense)
                        .Sum(t => (decimal?)t.Amount) ?? 0
                })
                .ToListAsync(cancellationToken);

            var mostUsedCategory = summary.OrderByDescending(c => c.TotalCategories)
                                    .FirstOrDefault()?.Name;
            var highestSpendingCategory = summary.OrderByDescending(c => c.HighestSpending)
                                                            .FirstOrDefault()?.Name;

            return new CategorySummaryResponse(
                totalCategories: summary.Count,
                mostUsedCategory: mostUsedCategory ?? "N/A",
                highestSpendingCategory: highestSpendingCategory ?? "N/A"
            ); 
        }
	}
}
