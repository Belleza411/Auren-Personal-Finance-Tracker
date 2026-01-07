using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.Category;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Auren.API.Controllers
{
	[Route("api/categories")]
	[ApiController]
    [EnableRateLimiting("fixed")]
    [Authorize]
	public class CategoriesController(ICategoryService categoryService) : ControllerBase
	{
		[HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories(
            [FromQuery] CategoriesFilter categoriesFilter,
            [FromQuery] int pageSize = 5, 
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var categories = await categoryService.GetCategories(userId.Value, categoriesFilter, pageSize, pageNumber, cancellationToken);
            return Ok(categories.Value);
        }

        [HttpGet("{categoryId:guid}")]
        public async Task<ActionResult<Category>> GetCategoryById([FromRoute] Guid categoryId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var category =  await categoryService.GetCategoryById(categoryId, userId.Value, cancellationToken);

            return category.IsSuccess ? Ok(category.Value) : NotFound($"Category with ID {categoryId} not found.");
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var createdCategory = await categoryService.CreateCategory(categoryDto, userId.Value, cancellationToken);

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
        public async Task<ActionResult<Category>> UpdateCategory([FromRoute] Guid categoryId, [FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var updatedCategory = await categoryService.UpdateCategory(categoryId, userId.Value, categoryDto, cancellationToken);

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
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid categoryId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var deleted = await categoryService.DeleteCategory(categoryId, userId.Value, cancellationToken);

            return deleted.IsSuccess ? NoContent() : NotFound($"Category with ID {categoryId} not found.");
        }

        [HttpGet("overview")]
        public async Task<ActionResult<IEnumerable<CategoryOverviewResponse>>> GetCategoryOverview(
            [FromQuery] CategoryOverviewFilter filter,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var overview = await categoryService.GetCategoryOverview(userId.Value, filter, pageSize, pageNumber,
                cancellationToken);
            return Ok(overview.Value);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<CategorySummaryResponse>> GetCategoriesSummary(CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var summary = await categoryService.GetCategoriesSummary(userId.Value, cancellationToken);
            return Ok(summary.Value);
        }
    }
}
