using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Models;
using Auren.Application.Common.Result;
using Auren.Application.Common.Specifications.Categories;
using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Categories.Queries.GetCategories
{
    public class GetCategoriesHandler(IAppDbContext db)
    {
        public async Task<Result<PagedResult<Category>>> Handle(
            GetCategoriesQuery query,
            CancellationToken ct)
        {
            var spec = new CategoryFilterSpecification(query.UserId, query.Filter);

            var baseQuery = db.Categories.Where(spec.ToExpression());

            var totalCount = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .OrderBy(c => c.Name)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(ct);

            return Result.Success(new PagedResult<Category>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            });
        }
    }
}
