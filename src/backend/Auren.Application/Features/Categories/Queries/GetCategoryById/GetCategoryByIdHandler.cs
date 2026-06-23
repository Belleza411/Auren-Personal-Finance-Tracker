using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Constants;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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

        //public async Task<List<Category>> SeedDefaultCategoryToUserAsync(List<Category> categories, CancellationToken cancellationToken)
        //{
        //    _dbContext.Categories.AddRange(categories);
        //    await _dbContext.SaveChangesAsync(cancellationToken);

        //    return categories;
        //}
        //public async Task<Result<List<Category>>> SeedDefaultCategoryToUser(Guid userId, CancellationToken cancellationToken)
        //{
        //    var categories = CategorySeeder.DefaultCategories.Select(c => new Category
        //    {
        //        Id = Guid.NewGuid(),
        //        UserId = userId,
        //        Name = c.Name,
        //        TransactionType = c.transactionType,
        //        CreatedAt = DateTime.UtcNow
        //    }).ToList();

        //    return Result.Success<List<Category>>(await _categoryRepository.SeedDefaultCategoryToUserAsync(categories, cancellationToken));
        //}
    }
}
