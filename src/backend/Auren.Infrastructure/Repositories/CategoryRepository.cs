using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Category;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Specifications.Categories;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using CloudinaryDotNet;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Auren.Infrastructure.Repositories
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
        private readonly AurenDbContext _dbContext;
        private readonly string _connectionString;

        public CategoryRepository(AurenDbContext dbContext, IConfiguration configuration) : base(dbContext)
		{
            _dbContext = dbContext;
            _connectionString = configuration.GetConnectionString("AurenDbConnection")
								?? throw new ArgumentNullException("Connection not found");
        }

		public async Task<PagedResult<Category>> GetCategoriesAsync(
            Guid userId,
            CategoriesFilter filter,
            int pageSize = 5,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
		{
            var skip = (pageNumber - 1) * pageSize;

            var spec = new CategoryFilterSpecification(userId, filter);
            var query = _dbContext.Categories
                .Where(spec.ToExpression());

            var totalCount = await query.CountAsync(cancellationToken);

            var categories = await query
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PagedResult<Category>
            {
                Items = categories,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
		}

        public async Task<List<Category>> SeedDefaultCategoryToUserAsync(List<Category> categories, CancellationToken cancellationToken)
        {
            _dbContext.Categories.AddRange(categories);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return categories;
        }

        public async Task<Category?> GetCategoryByNameAsync(Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken)
            =>  await _dbContext.Categories.FirstOrDefaultAsync(c => c.UserId == userId
                    && c.Name.Equals(categoryDto.Name)
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
                    TotalCategories = _dbContext.Transactions.Count(t => t.CategoryId == c.Id),
                    HighestSpending = _dbContext.Transactions
                        .Where(t => t.CategoryId == c.Id && t.TransactionType == TransactionType.Expense)
                        .Sum(t => (decimal?)t.Amount) ?? 0
                })
                .ToListAsync(cancellationToken);

            var mostUsedCategory = summary.OrderByDescending(c => c.TotalCategories)
                                    .FirstOrDefault()?.Name;
            var highestSpendingCategory = summary.OrderByDescending(c => c.HighestSpending)
                                                            .FirstOrDefault()?.Name;

            return new CategorySummaryResponse(
                TotalCategories: summary.Count,
                MostUsedCategory: mostUsedCategory ?? "N/A",
                HighestSpendingCategory: highestSpendingCategory ?? "N/A"
            ); 
        }

        public async Task<IReadOnlyList<Guid>> GetIdsByNamesAsync(Guid userId, IEnumerable<string> categories,  CancellationToken cancellationToken)
        {
            if (categories == null)
                return [];

            return await _dbContext.Categories
              .Where(c =>
                  c.UserId == userId &&
                  categories.Contains(c.Name))
              .Select(c => c.Id)
              .ToListAsync(cancellationToken);
        }
	}
}
