using Auren.API.DTOs.Requests;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
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
		private readonly ICategoryRepository _categoryRepository;

		public CategoriesController(ILogger<CategoriesController> logger, ICategoryRepository categoryRepository)
		{
			_logger = logger;
			_categoryRepository = categoryRepository;
		}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories(CancellationToken cancellationToken, [FromQuery] int? pageSize = 5, [FromQuery] int? pageNumber = 1)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var categories = await _categoryRepository.GetCategoriesAsync(userId.Value, cancellationToken, pageSize, pageNumber);
                return Ok(categories);
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
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(categoryId, userId.Value, cancellationToken);
                if (category == null)
                {
                    return NotFound($"Category id of {categoryId} not found. ");
                }
                return Ok(category);
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
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCategory = await _categoryRepository.CreateCategoryAsync(categoryDto, userId.Value, cancellationToken);
                
                return CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.CategoryId }, createdCategory);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid category data provided for user {UserId}", userId);
                return BadRequest(ex.Message);
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
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedCategory = await _categoryRepository.UpdateCategoryAsync(categoryId, userId.Value, categoryDto, cancellationToken);
                if (updatedCategory == null)
                {
                    return NotFound($"Category with ID {categoryId} not found.");
                }
                return Ok(updatedCategory);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid category data provided for user {UserId}", userId);
                return BadRequest(ex.Message);
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
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var deleted = await _categoryRepository.DeleteCategoryAsync(categoryId, userId.Value, cancellationToken);

                if (!deleted)
                {
                    return NotFound($"Category with ID {categoryId} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category {CategoryId} for user {UserId}", categoryId, userId);
                return StatusCode(500, "An error occurred while deleting the category. Please try again later.");
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User ID claim not found in token");
                return null;
            }

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            _logger.LogWarning("Invalid user ID format in claim: {UserIdClaim}", userIdClaim);
            return null;
        }
    }
}
