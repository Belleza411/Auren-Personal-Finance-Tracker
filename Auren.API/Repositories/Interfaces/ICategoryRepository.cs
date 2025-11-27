using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;

namespace Auren.API.Repositories.Interfaces
{
	public interface ICategoryRepository
	{
        Task<IEnumerable<Category>> GetCategoriesAsync(Guid userId,  CategoriesFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<Category?> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken);
        Task<Category> CreateCategoryAsync(Category category, Guid userId, CancellationToken cancellationToken);
        Task<Category?> UpdateCategoryAsync(Guid userId, Category category, CancellationToken cancellationToken);
        Task<bool> DeleteCategoryAsync(Category category, Guid userId, CancellationToken cancellationToken);
        Task<List<Category>> SeedDefaultCategoryToUserAsync(List<Category> categories, Guid userId, CancellationToken cancellationToken);
        Task<Category?> GetCategoryByNameAsync(Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken);
        Task<IEnumerable<CategoryOverviewResponse>> GetCategoryOverviewAsync(Guid userId, CategoryOverviewFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<CategorySummaryResponse> GetCategoriesSummaryAsync(Guid userId, CancellationToken cancellationToken);
    }
}
