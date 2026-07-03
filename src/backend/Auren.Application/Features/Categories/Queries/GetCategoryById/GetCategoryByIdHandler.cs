using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Features.Categories.Queries.GetCategoryById
{
    public class GetCategoryByIdHandler(IAppDbContext db)
    {
        public async Task<Result<Category>> Handle(
            GetCategoryByIdCommand cmd,
            CancellationToken ct)
        {
            var category = await db.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == cmd.CategoryId &&
                    c.UserId == cmd.UserId,
                    ct);

            return category != null
                ? Result.Success<Category>(category)
                : Result.Failure<Category>(Error.NotFound("Category not found"));
        }
    }
}
