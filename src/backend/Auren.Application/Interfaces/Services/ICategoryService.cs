using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Category;
using Auren.Domain.Entities;

namespace Auren.Application.Interfaces.Services
{
	public interface ICategoryService
	{
        Task<Result<Category>> CreateCategory(CategoryDto categoryDto, Guid userId, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteCategory(Guid categoryId, Guid userId, CancellationToken cancellationToken);
        Task<Result<PagedResult<Category>>> GetCategories(Guid userId, CategoriesFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<Result<Category?>> GetCategoryById(Guid categoryId, Guid userId, CancellationToken cancellationToken);
        Task<Result<Category>> UpdateCategory(Guid categoryId, Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken);
        Task<Result<List<Category>>> SeedDefaultCategoryToUser(Guid userId, CancellationToken cancellationToken);
        Task<Result<Category?>> GetCategoryByName(Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken);
        Task<Result<IEnumerable<CategoryOverviewResponse>>> GetCategoryOverview(
           Guid userId,
           CategoryOverviewFilter filter,
           int pageSize = 5,
           int pageNumber = 1,
           CancellationToken cancellationToken = default);
        Task<Result<CategorySummaryResponse>> GetCategoriesSummary(Guid userId, CancellationToken cancellationToken);
        Task<Result<IEnumerable<ExpenseCategoryChartResponse>>> GetExpenseCategoryChart(Guid userId, CancellationToken cancellationToken);
    }
}
