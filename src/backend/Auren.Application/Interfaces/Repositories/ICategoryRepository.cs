using Auren.Application.Common.Models;
using Auren.Application.Features.Categories.DTOs;
using Auren.Domain.Entities;

namespace Auren.Application.Interfaces.Repositories
{
	public interface ICategoryRepository : IRepository<Category>
	{
        Task<PagedResult<Category>> GetCategoriesAsync(Guid userId, CategoriesFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<List<Category>> SeedDefaultCategoryToUserAsync(List<Category> categories, CancellationToken cancellationToken);
        Task<Category?> GetCategoryByNameAsync(Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken);
        Task<IReadOnlyList<Guid>> GetIdsByNamesAsync(Guid userId, IEnumerable<string> categories, CancellationToken cancellationToken);
    }
}
