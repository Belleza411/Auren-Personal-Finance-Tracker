using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Category;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading;

namespace Auren.API.Controllers
{
	[Route("api/dashboard")]
	[ApiController]
    public class DashboardController(
         ITransactionService transactionService,
		 ICategoryService categoryService,
		 IGoalService goalService
        ) : ControllerBase
	{
		[HttpGet("summary")]
		public async Task<ActionResult<DashboardSummaryResponse>> GetDashboardSummary(
            [FromQuery] TimePeriod timePeriod = TimePeriod.ThisMonth, 
            CancellationToken cancellationToken = default)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var summary = await transactionService.GetDashboardSummary(userId.Value, timePeriod, cancellationToken);
            return Ok(summary.Value);
        }

        [HttpGet("goals/summary")]
        public async Task<ActionResult<CategorySummaryResponse>> GetGoalsSummary(CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

			var summary = await goalService.GetGoalsSummary(userId.Value, cancellationToken);
            return Ok(summary.Value);
        }

		[HttpGet("categories-expense")]
		public async Task<ActionResult<IEnumerable<ExpenseCategoryChartResponse>>> GetExpenseByCategoryChart(CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
       
            var chart = await categoryService.GetExpenseCategoryChart(userId.Value, cancellationToken);
            return Ok(chart.Value);
        }
    }
}
