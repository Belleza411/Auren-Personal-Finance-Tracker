using Auren.API.Data;
using Auren.API.DTOs.Requests;
using Auren.API.Helpers;
using Auren.API.Models.Domain;
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

		public async Task<IEnumerable<Category>> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken, int? pageSize, int? pageNumber)
		{
			try
			{
                var skip = (pageNumber - 1) * pageSize;

				var category = await _dbContext.Categories
					.Where(c => c.UserId == userId)
                    .OrderBy(c => c.CreatedAt)
                    .Skip(skip ?? 1)
                    .Take(pageSize ?? 5)
                    .AsNoTracking()
					.ToListAsync(cancellationToken);

				if(!category.Any())
				{
					_logger.LogWarning("No category found for {UserId}", userId);
				}

				return category;
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
    }
}
