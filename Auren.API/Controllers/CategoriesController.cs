using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.Extensions;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Auren.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auren.API.Controllers
{
	[Route("api/categories")]
	[ApiController]
	public class CategoriesController : ControllerBase
	{
		private readonly ILogger<CategoriesController> _logger;
        private readonly ICategoryService _categoryService;

		public CategoriesController(ILogger<CategoriesController> logger, ICategoryService categoryService)
		{
			_logger = logger;
			_categoryService = categoryService;
		}

		[HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories(CancellationToken cancellationToken,
            [FromQuery] CategoriesFilter categoriesFilter,
            [FromQuery] int pageSize = 5, 
            [FromQuery] int pageNumber = 1)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var categories = await _categoryService.GetCategories(userId.Value, categoriesFilter, pageSize, pageNumber, cancellationToken);
                return Ok(categories.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve categories for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving categories. Please try again later.");
            }
        }
        [HttpGet("{categoryId:guid}")]
        public async Task<ActionResult<Category>> GetCategoryById([FromRoute] Guid categoryId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var category =  await _categoryService.GetCategoryById(categoryId, userId.Value, cancellationToken);

                return category.IsSuccess ? Ok(category.Value) : NotFound($"Category with ID {categoryId} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve category {CategoryId} for user {UserId}", categoryId, userId);
                return StatusCode(500, "An error occurred while retrieving the category. Please try again later.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
           

            try
            {
                var createdCategory = await _categoryService.CreateCategory(categoryDto, userId.Value, cancellationToken);

                if (!createdCategory.IsSuccess)
                {
                    return createdCategory.Error.Code switch
                    {
                        ErrorType.InvalidInput
                            or ErrorType.ValidationFailed
                                => BadRequest(createdCategory.Error),

                        ErrorType.CategoryAlreadyExists => Conflict(createdCategory.Error),
                        ErrorType.CreateFailed => StatusCode(500, createdCategory.Error),

                        _ => StatusCode(500, "An unexpected error occurred.")
                    };
                }

                return CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.Value.CategoryId }, createdCategory.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create category for user {UserId}", userId);
                return StatusCode(500, "An error occurred while creating the category. Please try again later.");
            }
        }

        [HttpPut("{categoryId:guid}")]
        public async Task<ActionResult<Category>> UpdateCategory([FromRoute] Guid categoryId, [FromBody] CategoryDto categoryDto, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var updatedCategory = await _categoryService.UpdateCategory(categoryId, userId.Value, categoryDto, cancellationToken);

                if (!updatedCategory.IsSuccess)
                {
                    return updatedCategory.Error.Code switch
                    {
                        ErrorType.InvalidInput
                            or ErrorType.ValidationFailed
                                => BadRequest(updatedCategory.Error),

                        ErrorType.NotFound => NotFound(updatedCategory.Error),
                        ErrorType.UpdateFailed => StatusCode(500, updatedCategory.Error),

                        _ => StatusCode(500, "An unexpected error occurred.")
                    };
                }

                return Ok(updatedCategory.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update category {CategoryId} for user {UserId}", categoryId, userId);
                return StatusCode(500, "An error occurred while updating the category. Please try again later.");
            }
        }

        [HttpDelete("{categoryId:guid}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid categoryId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var deleted = await _categoryService.DeleteCategory(categoryId, userId.Value, cancellationToken);

                return deleted.IsSuccess ? NoContent() : NotFound($"Category with ID {categoryId} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {CategoryId} for user {UserId}", categoryId, userId);
                return StatusCode(500, "An error occurred while deleting the category. Please try again later.");
            }
        }

    }
}
