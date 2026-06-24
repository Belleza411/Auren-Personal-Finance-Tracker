using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Categories.DTOs;
using Auren.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Auren.Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryHandler(
        IAppDbContext db,
        IValidator<CategoryDto> validator)
    {
        public async Task<Result<Category>> Handle(
            UpdateCategoryCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Category>(Error.ValidationFailed(errors));
            }

            var category = await db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == cmd.CategoryId &&
                    c.UserId == cmd.UserId,
                    ct);

            if(category == null)
                return Result.Failure<Category>(Error.NotFound($"Category with id {cmd.CategoryId} not found"));

            category.Name = cmd.Dto.Name;
            category.TransactionType = cmd.Dto.TransactionType;

            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(category)
                : Result.Failure<Category>(Error.UpdateFailed("Failed to update category"));
        }
    }
}
