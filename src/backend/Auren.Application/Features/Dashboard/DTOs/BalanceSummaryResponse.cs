namespace Auren.Application.Features.Dashboard.DTOs
{
	public sealed record BalanceSummaryResponse(decimal Income, decimal Expense, decimal Balance);
}
