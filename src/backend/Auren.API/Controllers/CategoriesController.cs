using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Categories.Commands.CreateCategory;
using Auren.Application.Features.Categories.Commands.DeleteCategory;
using Auren.Application.Features.Categories.Commands.UpdateCategory;
using Auren.Application.Features.Categories.DTOs;
using Auren.Application.Features.Categories.Queries.GetCategories;
using Auren.Application.Features.Categories.Queries.GetCategoryById;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Auren.API.Controllers
{
	[Route("api/categories")]
	[ApiController]
    [Authorize]
	public class CategoriesController : ControllerBase
	{
		[HttpGet]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories(
            [FromServices] GetCategoriesHandler handler,
            [FromQuery] CategoriesFilter categoriesFilter,
            [FromQuery] int pageSize = 5, 
            [FromQuery] int pageNumber = 1,
            CancellationToken ct = default)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = new GetCategoriesQuery(userId.Value, categoriesFilter, pageSize, pageNumber);

            var categories = await handler.Handle(query, ct);
            return Ok(categories.Value);
        }

        [HttpGet("{categoryId:guid}")]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<Category>> GetCategoryById(
            [FromServices] GetCategoryByIdHandler handler,
            Guid categoryId,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new GetCategoryByIdCommand(userId.Value, categoryId);

            var category = await handler.Handle(cmd, ct);

            return category.IsSuccess ? Ok(category.Value) : NotFound($"Category with ID {categoryId} not found.");
        }

        [HttpPost]
        [EnableRateLimiting("write")]
        public async Task<ActionResult<Category>> CreateCategory(
            [FromServices] CreateCategoryHandler handler,
            CategoryDto categoryDto,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            ;
            var cmd = new CreateCategoryCommand(categoryDto, userId.Value);

            var createdCategory = await handler.Handle(cmd, ct);

            if (!createdCategory.IsSuccess)
            {
                return createdCategory.Error.Code switch
                {
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                            => BadRequest(createdCategory.Error),

                    ErrorTypes.CategoryAlreadyExists => Conflict(createdCategory.Error),
                    ErrorTypes.CreateFailed => StatusCode(500, createdCategory.Error),

                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }

            return CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.Value.Id }, createdCategory.Value);
        }

        [HttpPut("{categoryId:guid}")]
        [EnableRateLimiting("write")]
        public async Task<ActionResult<Category>> UpdateCategory(
            [FromServices] UpdateCategoryHandler handler,
            Guid categoryId,
            CategoryDto categoryDto,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new UpdateCategoryCommand(categoryId, userId.Value, categoryDto);

            var updatedCategory = await handler.Handle(cmd, ct);

            if (!updatedCategory.IsSuccess)
            {
                return updatedCategory.Error.Code switch
                {
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                            => BadRequest(updatedCategory.Error),

                    ErrorTypes.NotFound => NotFound(updatedCategory.Error),
                    ErrorTypes.UpdateFailed => StatusCode(500, updatedCategory.Error),

                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }

            return Ok(updatedCategory.Value);
        }

        [HttpDelete("{categoryId:guid}")]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> DeleteCategory(
            [FromServices] DeleteCategoryHandler handler,
            Guid categoryId,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new DeleteCategoryCommand(userId.Value, categoryId);

            var deleted = await handler.Handle(cmd, ct);

            return deleted.IsSuccess ? NoContent() : NotFound($"Category with ID {categoryId} not found.");
        }
    }
}
