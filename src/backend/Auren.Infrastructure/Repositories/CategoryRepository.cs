using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Category;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Specifications.Categories;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using CloudinaryDotNet;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Auren.Infrastructure.Repositories
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
        private readonly AurenDbContext _dbContext;

        public CategoryRepository(AurenDbContext dbContext) : base(dbContext)
		{
            _dbContext = dbContext;
        }

		public async Task<PagedResult<Category>> GetCategoriesAsync(
            Guid userId,
            CategoriesFilter filter,
            int pageSize = 5,
            int pageNumber = 1,
            CancellationToken cancellationToken = default)
		{
            var skip = (pageNumber - 1) * pageSize;

            IEnumerable<Guid> categoryIds = [];

            if (filter.Categories?.Any() == true)
            {
                categoryIds = await GetIdsByNamesAsync(userId, filter.Categories, cancellationToken);
            }

            var spec = new CategoryFilterSpecification(userId, filter, categoryIds);
            var query = _dbContext.Categories
                .Where(spec.ToExpression());

            var totalCount = await query.CountAsync(cancellationToken);

            var categories = await query
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new PagedResult<Category>
            {
                Items = categories,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
		}

        public async Task<List<Category>> SeedDefaultCategoryToUserAsync(List<Category> categories, CancellationToken cancellationToken)
        {
            _dbContext.Categories.AddRange(categories);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return categories;
        }

        public async Task<Category?> GetCategoryByNameAsync(Guid userId, CategoryDto categoryDto, CancellationToken cancellationToken)
            =>  await _dbContext.Categories.FirstOrDefaultAsync(c => c.UserId == userId
                    && c.Name.Equals(categoryDto.Name)
                    && c.TransactionType == categoryDto.TransactionType,
                    cancellationToken);

        public async Task<IReadOnlyList<Guid>> GetIdsByNamesAsync(Guid userId, IEnumerable<string> categories,  CancellationToken cancellationToken)
        {
            if (categories == null)
                return [];

            return await _dbContext.Categories
              .Where(c =>
                  c.UserId == userId &&
                  categories.Contains(c.Name))
              .Select(c => c.Id)
              .ToListAsync(cancellationToken);
        }
	}
}
