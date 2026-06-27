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
using Microsoft.AspNetCore.Http.HttpResults;
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

            var result = await handler.Handle(cmd, ct);

            return result.Match<ActionResult<Category>>(
                onSuccess: value => Ok(value),
                onFailure: _ => NotFound($"Category with ID {categoryId} not found.")
            );
        }

        [HttpPost]
        [EnableRateLimiting("sensitive")]
        public async Task<ActionResult<Category>> CreateCategory(
            [FromServices] CreateCategoryHandler handler,
            CategoryDto categoryDto,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            ;
            var cmd = new CreateCategoryCommand(categoryDto, userId.Value);

            var result = await handler.Handle(cmd, ct);

            return result.Match(
                onSuccess: value => CreatedAtAction(nameof(GetCategoryById), new { categoryId = value.Id }, value),
                onFailure: err => err.Code switch
                {
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                            => BadRequest(err),

                    ErrorTypes.CategoryAlreadyExists => Conflict(err),
                    ErrorTypes.CreateFailed => StatusCode(500, err),

                    _ => StatusCode(500, "An unexpected error occurred.")
                });
        }

        [HttpPut("{categoryId:guid}")]
        [EnableRateLimiting("sensitive")]
        public async Task<ActionResult<Category>> UpdateCategory(
            [FromServices] UpdateCategoryHandler handler,
            Guid categoryId,
            CategoryDto categoryDto,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new UpdateCategoryCommand(categoryId, userId.Value, categoryDto);

            var result = await handler.Handle(cmd, ct);

            return result.Match(
                onSuccess: Ok,
                onFailure: err => err.Code switch
                {
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                        => BadRequest(err),
                    ErrorTypes.NotFound => NotFound(err),
                    ErrorTypes.UpdateFailed => StatusCode(500, err),
                    _ => StatusCode(500, "An unexpected error occurred.")
                });
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

            var result = await handler.Handle(cmd, ct);

            return result.Match<IActionResult>(
                 onSuccess: NoContent,
                 onFailure: NotFound);
        }
    }
}
