using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;

namespace Auren.API.Services.Interfaces
{
	public interface ICategoryService
	{
        Task<Result<Category>> CreateCategory(CategoryDto categoryDto, Guid userId, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteCategory(Guid categoryId, Guid userId, CancellationToken cancellationToken);
        Task<Result<IEnumerable<Category>>> GetCategories(Guid userId, CategoriesFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
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
    }
}
