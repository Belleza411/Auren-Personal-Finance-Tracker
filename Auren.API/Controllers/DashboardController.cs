using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Responses;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace Auren.API.Controllers
{
	[Route("api/dashboard")]
	[ApiController]
	public class DashboardController : ControllerBase
	{
		private readonly ITransactionRepository _transactionRepository;
		private readonly IGoalRepository _goalRepository;
		private readonly ILogger<DashboardController> _logger;
		private readonly ITransactionService _transactionService;
		private readonly ICategoryService _categoryService;
		private readonly IGoalService _goalService;

		public DashboardController(ITransactionRepository transactionRepository, IGoalRepository goalRepository, ILogger<DashboardController> logger, ITransactionService transactionService, ICategoryService categoryService, IGoalService goalService)
		{
			_transactionRepository = transactionRepository;
			_goalRepository = goalRepository;
			_logger = logger;
			_transactionService = transactionService;
			_categoryService = categoryService;
			_goalService = goalService;
		}

		[HttpGet("transaction/average-daily-spending")]
		public async Task<ActionResult<AvgDailySpendingResponse>> GetAvgDailySpending(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var avgSpending = await _transactionService.GetAvgDailySpending(userId.Value, cancellationToken);

				return Ok(new AvgDailySpendingResponse(
					Math.Round(avgSpending.Value.avgSpending, 2),
					Math.Round(avgSpending.Value.pecentageChange, 2)
				));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve avg daily spending for user {UserId}", userId);
				return StatusCode(500, "An error occurred while retrieving avg daily spending. Please try again later.");
			}
		}

		[HttpGet("categories/categories-overview")]
        public async Task<ActionResult<IEnumerable<CategoryOverviewResponse>>> GetCategoryOverview(
			[FromQuery] CategoryOverviewFilter filter,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)  
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
				var overview = await _categoryService.GetCategoryOverview(userId.Value, filter, pageSize, pageNumber, 
					cancellationToken);
                return Ok(overview.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetOverview endpoint");
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }

		[HttpGet("summary")]
		public async Task<ActionResult<DashboardSummaryResponse>> GetDashboardSummary(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var summary = await _transactionService.GetDashboardSummary(userId.Value, cancellationToken);
                return Ok(summary.Value);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve dashboard summary for user {UserId}", userId);
				return StatusCode(500, "An error occurred while retrieving dashboard summary. Please try again later.");
            }
        }

		[HttpGet("categories/summary")]
		public async Task<ActionResult<CategorySummaryResponse>> GetCategoriesSummary(CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
				var summary = await _categoryService.GetCategoriesSummary(userId.Value, cancellationToken);
                return Ok(summary.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving category summary. Please try again later.");
            }
        }

        [HttpGet("goals/summary")]
        public async Task<ActionResult<CategorySummaryResponse>> GetGoalsSummary(CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
				var summary = await _goalService.GetGoalsSummary(userId.Value, cancellationToken);
                return Ok(summary.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving goals summary. Please try again later.");
            }
        }
    }
}
