using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryHandler(IAppDbContext db)
    {
        public async Task<Result<bool>> Handle(
            DeleteCategoryCommand cmd,
            CancellationToken ct)
        {
            var category = await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == cmd.CategoryId &&
                    c.UserId == cmd.UserId,
                    ct);

            if (category == null)
                return Result.Failure<bool>(Error.NotFound("Category not found"));

            db.Categories.Remove(category);
            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(true)
                : Result.Failure<bool>(Error.DeleteFailed("Failed to delete category"));
        }
    }
}
