using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Auren.API.Controllers
{
	[Route("api/dashboard")]
	[ApiController]
    public class DashboardController(
         IDashboardService dashboardService
        ) : ControllerBase
	{
		[HttpGet("summary")]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<DashboardSummaryResponse>> GetDashboardSummary(
            TimePeriod timePeriod = TimePeriod.ThisMonth, 
            CancellationToken cancellationToken = default)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var summary = await dashboardService.GetDashboardSummary(userId.Value, timePeriod, cancellationToken);
            return Ok(summary.Value);
        }

		[HttpGet("expense-breakdown")]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<ExpenseBreakdownResponse>> GetExpenseOverview(TimePeriod timePeriod = TimePeriod.ThisMonth, CancellationToken cancellationToken = default)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
       
            var chart = await dashboardService.GetExpenseBreakdown(userId.Value, timePeriod, cancellationToken);
            return Ok(chart.Value);
        }

        [HttpGet("income-vs-expense")]
        [EnableRateLimiting("read")]

        public async Task<ActionResult<IncomesVsExpenseResponse>> GetIncomesVsExpenses(
            TimePeriod timePeriod = TimePeriod.ThisMonth,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var incomesVsExpensesData = await dashboardService.GetIncomesVsExpenses(userId.Value, timePeriod, cancellationToken);

            return Ok(incomesVsExpensesData.Value);
        }
    }
}
