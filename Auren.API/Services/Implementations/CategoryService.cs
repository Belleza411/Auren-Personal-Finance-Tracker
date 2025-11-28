using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Auren.API.Services.Interfaces;
using FluentValidation;
using static Auren.API.Helpers.Result.Error;

namespace Auren.API.Services.Implementations
{
	public class CategoryService : ICategoryService
	{
        private readonly ILogger<CategoryService> _logger;
        private readonly IValidator<CategoryDto> _validator;
        private readonly ICategoryRepository _categoryRepository;

		public CategoryService(ILogger<CategoryService> logger, IValidator<CategoryDto> validator, ICategoryRepository categoryRepository)
		{
			_logger = logger;
			_validator = validator;
			_categoryRepository = categoryRepository;
		}

        public async Task<Result<Category>> CreateCategory(CategoryDto categoryDto, Guid userId, CancellationToken cancellationToken)
		{
            if (categoryDto == null)
            {
                return Result.Failure<Category>(Error.InvalidInput("All fields are required"));
            }

            var validationResult = await _validator.ValidateAsync(categoryDto, cancellationToken); 
            if(!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Category>(Error.ValidationFailed(errors));
            }

            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(userId, categoryDto, cancellationToken);
            
            if(existingCategory != null)
            {
                return Result.Failure<Category>(Error.CategoryError.AlreadyExists($"Category with the name of {categoryDto.Name} already exists"));
            }

            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                UserId = userId,
                Name = categoryDto.Name,
                TransactionType = categoryDto.TransactionType,
                CreatedAt = DateTime.UtcNow
            };

            var createdCategory = await _categoryRepository.CreateCategoryAsync(category, userId, cancellationToken);

            return createdCategory == null 
                ? Result.Failure<Category>(Error.CreateFailed("Failed to create category.")) 
                : Result.Success(createdCategory);
        }

        public async Task<Result<bool>> DeleteCategory(Guid categoryId, Guid userId, CancellationToken cancellationToken)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId, userId, cancellationToken);
            if(existingCategory == null)
            {
                return Result.Failure<bool>(Error.NotFound($"Category with id {categoryId} not found"));
            }
            await _categoryRepository.DeleteCategoryAsync(existingCategory, userId, cancellationToken);
            return Result.Success(true);
        }

        public async Task<Result<IEnumerable<Category>>> GetCategories(Guid userId, CategoriesFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default)
            => Result.Success(await _categoryRepository.GetCategoriesAsync(userId, filter, pageSize, pageNumber, cancellationToken));

        public async Task<Result<Category?>> GetCategoryById(Guid categoryId, Guid userId, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId, userId, cancellationToken);
            
            if(category == null)
            {
                return Result.Failure<Category?>(Error.NotFound($"Category with id {categoryId} not found"));
            }

            return Result.Success<Category?>(category);
        }

        public async Task<Result<Category>> UpdateCategory(Guid categoryId, Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            if (categoryDto == null)
            {
                return Result.Failure<Category>(Error.InvalidInput("All fields are required"));
            }

            var validationResult = await _validator.ValidateAsync(categoryDto, cancellationToken);
            if(!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Category>(Error.ValidationFailed(errors));
            }

            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId, userId, cancellationToken);
            if(existingCategory == null)
            {
                return Result.Failure<Category>(Error.NotFound($"Category with id {categoryId} not found"));
            }

            existingCategory.Name = categoryDto.Name;
            existingCategory.TransactionType = categoryDto.TransactionType;

            var updatedCategory = await _categoryRepository.UpdateCategoryAsync(userId, existingCategory, cancellationToken);

            return updatedCategory == null 
                ? Result.Failure<Category>(Error.UpdateFailed("Failed to update category. ")) 
                : Result.Success(updatedCategory);
        }

        public async Task<Result<List<Category>>> SeedDefaultCategoryToUser(Guid userId, CancellationToken cancellationToken)
        {
            var categories = CategorySeeder.DefaultCategories.Select(c => new Category
            {
                CategoryId = Guid.NewGuid(),
                UserId = userId,
                Name = c.Name,
                TransactionType = c.transactionType,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            return Result.Success<List<Category>>(await _categoryRepository.SeedDefaultCategoryToUserAsync(categories, userId, cancellationToken));
        }

        public async Task<Result<Category?>> GetCategoryByName(Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetCategoryByNameAsync(userId, categoryDto, cancellationToken);
            return category == null ?
                Result.Failure<Category?>(Error.NotFound($"Category with name {categoryDto.Name} not found")) :
                Result.Success<Category?>(category);
        }

        public async Task<Result<IEnumerable<CategoryOverviewResponse>>> GetCategoryOverview(
           Guid userId,
           CategoryOverviewFilter filter,
           int pageSize = 5,
           int pageNumber = 1,
           CancellationToken cancellationToken = default)
        {
            var categoryOverviews = await _categoryRepository.GetCategoryOverviewAsync(userId, filter, pageSize, pageNumber, cancellationToken);
            return Result.Success<IEnumerable<CategoryOverviewResponse>>(categoryOverviews);
        }

        public async Task<Result<CategorySummaryResponse>> GetCategoriesSummary(Guid userId, CancellationToken cancellationToken)
            => Result.Success<CategorySummaryResponse>(await _categoryRepository.GetCategoriesSummaryAsync(userId, cancellationToken));
    }
}
   