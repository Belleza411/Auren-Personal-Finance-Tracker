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

namespace Auren.Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryHandler(
        IAppDbContext db,
        IValidator<CategoryDto> validator)
    {
        public async Task<Result<Category>> Handle(
            CreateCategoryCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Category>(Error.ValidationFailed(errors));
            }

            var existingCategory = await GetCategoryByName(cmd.UserId, cmd.Dto, ct);

            if(existingCategory != null)
                return Result.Failure<Category>(Error.CategoryError.AlreadyExists($"Category with the name of {categoryDto.Name} already exists"));

            var newCategory = new Category
            {
                Id = Guid.NewGuid(),
                UserId = cmd.UserId,
                Name = cmd.Dto.Name,
                TransactionType = cmd.Dto.TransactionType,
                CreatedAt = DateTime.UtcNow
            };

            await db.Categories.AddAsync(newCategory, ct);
            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(newCategory)
                : Result.Failure<Category>(Error.CreateFailed("Failed to create category."));
        }

        private async Task<Category?> GetCategoryByName(
            Guid userId,
            CategoryDto dto,
            CancellationToken ct)
        {
            return await db.Categories
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.Name.Equals(dto.Name) &&
                    c.TransactionType == dto.TransactionType,
                    ct);
        }
    }
}
