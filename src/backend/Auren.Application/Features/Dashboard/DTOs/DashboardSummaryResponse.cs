namespace Auren.Application.Features.Dashboard.DTOs
{
	public sealed record DashboardSummaryResponse(
		TransactionMetricResponse TotalBalance,
		TransactionMetricResponse Income,
		TransactionMetricResponse Expense,
		TransactionMetricResponse AverageDailySpending);
}
