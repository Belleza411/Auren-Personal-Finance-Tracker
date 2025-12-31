using Auren.Application.Common.Result;
using Auren.Application.Constants;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.Category;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace Auren.Application.Services
{
	public class CategoryService : ICategoryService
	{
        private readonly IValidator<CategoryDto> _validator;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;

		public CategoryService(IValidator<CategoryDto> validator, ICategoryRepository categoryRepository, ITransactionRepository transactionRepository)
		{
			_validator = validator;
			_categoryRepository = categoryRepository;
			_transactionRepository = transactionRepository;
		}

		public async Task<Result<Category>> CreateCategory(CategoryDto categoryDto, Guid userId, CancellationToken cancellationToken)
		{
            if (categoryDto == null)
                return Result.Failure<Category>(Error.InvalidInput("All fields are required"));

            var validationResult = await _validator.ValidateAsync(categoryDto, cancellationToken); 
            if(!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Category>(Error.ValidationFailed(errors));
            }

            var existingCategory = await _categoryRepository.GetCategoryByNameAsync(userId, categoryDto, cancellationToken);
            
            if(existingCategory != null)
                return Result.Failure<Category>(Error.CategoryError.AlreadyExists($"Category with the name of {categoryDto.Name} already exists"));

            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                UserId = userId,
                Name = categoryDto.Name,
                TransactionType = categoryDto.TransactionType,
                CreatedAt = DateTime.UtcNow
            };

            var createdCategory = await _categoryRepository.CreateCategoryAsync(category, cancellationToken);

            return createdCategory == null 
                ? Result.Failure<Category>(Error.CreateFailed("Failed to create category.")) 
                : Result.Success(createdCategory);
        }

        public async Task<Result<bool>> DeleteCategory(Guid categoryId, Guid userId, CancellationToken cancellationToken)
        {
            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId, userId, cancellationToken);

            if(existingCategory == null)
                return Result.Failure<bool>(Error.NotFound($"Category with id {categoryId} not found"));

            var categoryToDelete = await _categoryRepository.DeleteCategoryAsync(existingCategory, cancellationToken);
            return categoryToDelete 
                ? Result.Success(true) 
                : Result.Failure<bool>(Error.DeleteFailed("Failed to delete category."));
        }

        public async Task<Result<IEnumerable<Category>>> GetCategories(Guid userId, CategoriesFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default)
            => Result.Success(await _categoryRepository.GetCategoriesAsync(userId, filter, pageSize, pageNumber, cancellationToken));

        public async Task<Result<Category?>> GetCategoryById(Guid categoryId, Guid userId, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId, userId, cancellationToken);
            
            if(category == null)
                return Result.Failure<Category?>(Error.NotFound($"Category with id {categoryId} not found"));

            return Result.Success<Category?>(category);
        }

        public async Task<Result<Category>> UpdateCategory(Guid categoryId, Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            if (categoryDto == null)
                return Result.Failure<Category>(Error.InvalidInput("All fields are required"));

            var validationResult = await _validator.ValidateAsync(categoryDto, cancellationToken);
            if(!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Category>(Error.ValidationFailed(errors));
            }

            var existingCategory = await _categoryRepository.GetCategoryByIdAsync(categoryId, userId, cancellationToken);
            if(existingCategory == null)
                return Result.Failure<Category>(Error.NotFound($"Category with id {categoryId} not found"));

            existingCategory.Name = categoryDto.Name;
            existingCategory.TransactionType = categoryDto.TransactionType;

            var updatedCategory = await _categoryRepository.UpdateCategoryAsync(existingCategory, cancellationToken);

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

            return Result.Success<List<Category>>(await _categoryRepository.SeedDefaultCategoryToUserAsync(categories, cancellationToken));
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
            => Result.Success<IEnumerable<CategoryOverviewResponse>>(await _categoryRepository.GetCategoryOverviewAsync(userId, filter, pageSize, pageNumber, cancellationToken));
        

        public async Task<Result<CategorySummaryResponse>> GetCategoriesSummary(Guid userId, CancellationToken cancellationToken)
            => Result.Success<CategorySummaryResponse>(await _categoryRepository.GetCategoriesSummaryAsync(userId, cancellationToken));

        public async Task<Result<IEnumerable<ExpenseCategoryChartResponse>>> GetExpenseCategoryChart(Guid userId, CancellationToken cancellationToken)
        {
            var transactionExpenses = new TransactionFilter { TransactionType = TransactionType.Expense };
            var expenses = await _transactionRepository.GetTransactionsAsync(
                userId, transactionExpenses, 5, 1, cancellationToken);

            var categories = await _categoryRepository.GetCategoriesAsync(userId, null!, int.MaxValue, 1, cancellationToken);

            var categoryLookup = categories.ToDictionary(c => c.CategoryId, c => c.Name);

            var totalAmount = expenses.Items.Sum(e => e.Amount);
            var chartData = expenses.Items
                .GroupBy(e => e.CategoryId)
                .Select(g =>
                {
                    var categoryName = categoryLookup.TryGetValue(g.Key, out var name)
                        ? name
                        : "Unknown";

                    return new ExpenseCategoryChartResponse(
                        Category: categoryName,
                        Amount: g.Sum(t => t.Amount),
                        Percentage: totalAmount == 0 ? 0 :
                            Math.Round((g.Sum(t => t.Amount) / totalAmount) * 100, 2)
                    );
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
                


            return Result.Success<IEnumerable<ExpenseCategoryChartResponse>>(chartData);
        }
    }
}
   