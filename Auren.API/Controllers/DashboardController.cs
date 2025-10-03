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
		public async Task<ActionResult<AvgDailySpendingResponse>> GetAvgDailySpending(CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
			{
				var month = DateTime.UtcNow;

				var avgSpending = await _transactionRepository.GetAvgDailySpendingAsync(userId.Value, month, cancellationToken);

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
	}
}
