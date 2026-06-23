namespace Auren.Application.Dashboard.DTOs
{
	public sealed record DashboardSummaryResponse(
		TransactionMetricResponse TotalBalance,
		TransactionMetricResponse Income,
		TransactionMetricResponse Expense,
		TransactionMetricResponse AverageDailySpending);
}
