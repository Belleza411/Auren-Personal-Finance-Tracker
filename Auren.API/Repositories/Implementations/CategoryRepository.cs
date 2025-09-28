using Auren.API.Data;
using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.Helpers;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auren.API.Repositories.Implementations
{
	public class CategoryRepository : ICategoryRepository
	{
        private readonly ILogger<TransactionRepository> _logger;
        private readonly AurenDbContext _dbContext;

		public CategoryRepository(ILogger<TransactionRepository> logger, AurenDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<Category> CreateCategoryAsync(CategoryDto categoryDto, Guid userId, CancellationToken cancellationToken)
		{
			if(categoryDto == null)
			{
                _logger.LogWarning("TransactionDto or Category is null for user {UserId}", userId);
                throw new ArgumentException("Transaction data and category are required");
            }

			try
			{
				var category = new Category {
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

            if (filter.IsExpense == true)
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
    }
}
