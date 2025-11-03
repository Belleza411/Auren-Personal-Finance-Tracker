using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;

namespace Auren.API.Repositories.Interfaces
{
	public interface ICategoryRepository
	{
        Task<IEnumerable<Category>> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken, CategoriesFilter filter, int? pageSize, int? pageNumber);
        Task<Category?> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken);
        Task<Category> CreateCategoryAsync(CategoryDto categoryDto, Guid userId, CancellationToken cancellationToken);
        Task<Category?> UpdateCategoryAsync(Guid categoryId, Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken);
        Task<bool> DeleteCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken);
        Task<List<Category>> SeedDefaultCategoryToUserAsync(Guid userId, CancellationToken cancellationToken);
        Task<Category?> GetCategoryByNameAsync(Guid userId, CancellationToken cancellationToken, CategoryDto categoryDto);
        Task<IEnumerable<CategoryOverviewResponse>> GetCategoryOverviewAsync(Guid userId, CancellationToken cancellationToken, CategoryOverviewFilter filter, int? pageSize, int? pageNumber);
    }
}
