using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Application.Features.Dashboard.Queries.GetDashboardSummary;
using Auren.Application.Features.Dashboard.Queries.GetExpenseBreakdown;
using Auren.Application.Features.Dashboard.Queries.GetIncomesVsExpenses;
using Auren.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Auren.API.Controllers
{
	[Route("api/dashboard")]
	[ApiController]
    public class DashboardController : ControllerBase
	{
		[HttpGet("summary")]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<DashboardSummaryResponse>> GetDashboardSummary(
            [FromServices] GetDashboardSummaryHandler handler,
            TimePeriod timePeriod = TimePeriod.ThisMonth, 
            CancellationToken ct = default)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

            var query = new GetDashboardSummaryQuery(userId.Value, timePeriod);

            var summary = await handler.Handle(query, ct);
            return Ok(summary.Value);
        }

		[HttpGet("expense-breakdown")]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<ExpenseBreakdownResponse>> GetExpenseBreakdown(
            [FromServices] GetExpenseBreakdownHandler handler,
            TimePeriod timePeriod = TimePeriod.ThisMonth,
            CancellationToken ct = default)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = new GetExpenseBreakdownQuery(userId.Value, timePeriod);

            var chart = await handler.Handle(query, ct);

            return Ok(chart.Value);
        }

        [HttpGet("income-vs-expense")]
        [EnableRateLimiting("read")]

        public async Task<ActionResult<IncomesVsExpenseResponse>> GetIncomesVsExpenses(
            [FromServices] GetIncomesVsExpensesHandler handler,
            TimePeriod timePeriod = TimePeriod.ThisMonth,
            CancellationToken ct = default)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = new GetIncomesVsExpensesQuery(userId.Value, timePeriod);

            var incomesVsExpensesData = await handler.Handle(query, ct);

            return Ok(incomesVsExpensesData.Value);
        }
    }
}
