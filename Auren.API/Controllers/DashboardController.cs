using Auren.API.DTOs.Responses;
using Auren.API.Extensions;
using Auren.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auren.API.Controllers
{
	[Route("api/dashboard")]
	[ApiController]
	public class DashboardController : ControllerBase
	{
		private readonly ITransactionRepository _transactionRepository;
		private readonly ILogger<DashboardController> _logger;

		public DashboardController(ITransactionRepository transactionRepository, ILogger<DashboardController> logger)
		{
			_transactionRepository = transactionRepository;
			_logger = logger;
		}

		[HttpGet("average-daily-spending")]
		public async Task<ActionResult<AvgDailySpendingResponse>> GetAvgDailySpending([FromQuery] DateTime? month, CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var targetMonth = month ?? DateTime.Today;

				var avgSpending = await _transactionRepository.GetAvgDailySpendingAsync(userId.Value, targetMonth, cancellationToken);

				return Ok(new AvgDailySpendingResponse(
					Math.Round(avgSpending.avgSpending, 2),
					Math.Round(avgSpending.pecentageChange, 2)
				));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve avg daily spending for user {UserId}", userId);
				return StatusCode(500, "An error occurred while retrieving avg daily spending. Please try again later.");
			}
		}

		[HttpGet("income-vs-expense")]
		public async Task<IActionResult> GetIncomeVsExpenseChart(
			[FromQuery] DateTime? startMonth,
			[FromQuery] DateTime? endMonth,
			CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

            try
			{
				var start = startMonth ?? new DateTime(DateTime.Today.Year, 1, 1);
				var end = endMonth ?? new DateTime(DateTime.Today.Year, 12, 1);

				if (start > end) return BadRequest("Start month cannot be after end month");

				var result = await _transactionRepository.GetIncomeVsExpenseChartAsync(userId.Value, start, end, cancellationToken);

				return Ok(result);
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve income vs expense chart data for user {UserId}", userId);
				return StatusCode(500, "An error occurred while retrieving income vs expense chart data. Please try again later.");
            }
        }
	}
}
