using Auren.API.Data;
using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Auren.API.Repositories.Implementations
{
	public class CategoryRepository : ICategoryRepository
	{
        private readonly ILogger<TransactionRepository> _logger;
        private readonly AurenDbContext _dbContext;
        private readonly string _connectionString;

        public CategoryRepository(ILogger<TransactionRepository> logger, AurenDbContext dbContext, IConfiguration configuration)
		{
			_logger = logger;
			_dbContext = dbContext;
            _connectionString = configuration.GetConnectionString("AurenDbConnection") ?? throw new ArgumentNullException("Connection string not found.");
        }

		public async Task<Category> CreateCategoryAsync(CategoryDto categoryDto, Guid userId, CancellationToken cancellationToken)
		{
			if(categoryDto == null || string.IsNullOrEmpty(categoryDto.Name))
			{
                _logger.LogWarning("TransactionDto or Category is null for user {UserId}", userId);
                throw new ArgumentException("Transaction data and category are required");
            }

			try
			{
                var existingCategory = await GetCategoryByNameAsync(userId, cancellationToken, categoryDto);

                if (existingCategory != null)
                {
                    _logger.LogWarning("Category '{CategoryName}' with type {TransactionType} already exists for user {UserId}",
                        categoryDto.Name, categoryDto.TransactionType, userId);
                    throw new InvalidOperationException($"Category '{categoryDto.Name}' already exists.");
                }

                var category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    UserId = userId,
                    Name = categoryDto.Name,
                    TransactionType = categoryDto.TransactionType,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Categories.AddAsync(category, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("{Category} with {CategoryId} was created succesfully for {UserId}", category, category.CategoryId, userId);

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create category for user {UserId}", userId);
                throw;
            }
        }

		public async Task<bool> DeleteCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
		{
            try
            {
                var category = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId, cancellationToken);

                if (category == null)
                {
                    _logger.LogWarning("Category with id of {CategoryId} not found for {UserId}", categoryId, userId);
                    return false;
                }

                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Category with id of {CategoryId} was successfully deleted for {UserId}", categoryId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category for user {UserId} with CategoryId of {CategoryId}", userId, categoryId);
                return false;
            }
        }

		public async Task<IEnumerable<Category>> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken, CategoriesFilter filter, int? pageSize, int? pageNumber)
		{
			try
			{
                var skip = ((pageNumber ?? 1) - 1) * (pageSize ?? 5);

                var query = _dbContext.Categories
                    .Where(c => c.UserId == userId);

                if(HasActiveFilter(filter))
                {
                    var filteredQuery = ApplyFilters(query, filter);
                    
                    if (filteredQuery is IQueryable<Category> categoryQuery)
                    {
                        query = categoryQuery;
                    }
                }

                var categories = await query
                    .Skip(skip)
                    .Take(pageSize ?? 5)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} categories for user {UserId}", categories.Count, userId);

                return categories;
            }
			catch(Exception ex)
			{
                _logger.LogError(ex, "Failed to retrieve categories for user {UserId}", userId);
                throw;
            }
		}

		public async Task<Category?> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
		{
			try
			{
                var category = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId, cancellationToken);

                if (category == null)
				{
					_logger.LogWarning("Category with the id of {CategoryId} is not found for {UserId}", categoryId, userId);
				}

				return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve category with the id of {CategoryId} for user {UserId}", categoryId, userId);
                throw;
            }
        }

		public async Task<Category?> UpdateCategoryAsync(Guid categoryId, Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken)
		{
			if(categoryDto == null)
			{
				_logger.LogWarning("Provided category was empty or null for {UserId}", userId);
                throw new ArgumentException("Category data are required");
            }
			
			try
			{
                var category = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId, cancellationToken);

                if (category == null)
                {
                    _logger.LogWarning("Category with the id of {CategoryId} is not found for {UserId}", categoryId, userId);
					return null;
				}

				category.Name = categoryDto.Name;
				category.TransactionType = categoryDto.TransactionType;

				await _dbContext.SaveChangesAsync(cancellationToken);
				_logger.LogInformation("Category with id of {CategoryId} for {UserId} was successfull updated. ", categoryId, userId);

				return category;
			}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update category with the id of {CategoryId} for user {UserId}", categoryId, userId);
                throw;
            }
        }

        public async Task<List<Category>> SeedDefaultCategoryToUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var categories = CategorySeeder.DefaultCategories.Select(c => new Category
                {
                    CategoryId = Guid.NewGuid(),
                    UserId = userId,
                    Name = c.Name,
                    TransactionType = c.transactionType,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _dbContext.Categories.AddRange(categories);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed default categories for user {UserId}", userId);
                throw;
            }
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

		public async Task<Category?> GetCategoryByNameAsync(Guid userId, CancellationToken cancellationToken, CategoryDto categoryDto)
		{
			try
            {
                var existingCategory = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.UserId == userId
                        && c.Name.ToLower() == categoryDto.Name.ToLower()
                        && c.TransactionType == categoryDto.TransactionType,
                        cancellationToken);

                if (existingCategory == null) return null;

                return existingCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve category with the name of {CategoryName} for user {UserId}", categoryDto.Name, userId);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryOverviewResponse>> GetCategoryOverviewAsync(
            Guid userId,
            CancellationToken cancellationToken,
            CategoryOverviewFilter filter,
            int? pageSize, int? pageNumber)
        {
            var offset = (pageNumber - 1) * pageSize;

            try
            {
                var sql = @"
                    SELECT 
                        c.Name AS Category,
                        c.TransactionType,
                        ISNULL(AVG(t.Amount), 0) AS AverageSpending,
                        ISNULL(COUNT(t.TransactionId), 0) AS TransactionCount
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
                    Category = filter.Category,
                    MinAmount = filter.MinAmount,
                    MaxAmount = filter.MaxAmount,
                    MinTransactionCount = filter.MinTransactionCount,
                    MaxTransactionCount = filter.MinTransactionCount,
                    Offset = offset,
                    pageSize = pageSize ?? 5,

                });

                if (result == null)
                {
                    _logger.LogWarning("Query returned null result for category overview");
                    return Enumerable.Empty<CategoryOverviewResponse>();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve category overview for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CategorySummaryResponse> GetCategoriesSummaryAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve categories summary for user {UserId}", userId);
                throw;
            }
        }
	}
}
