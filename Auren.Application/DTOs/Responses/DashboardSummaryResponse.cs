namespace Auren.Application.DTOs.Responses
{
	public sealed record DashboardSummaryResponse(TransactionMetricResponse TotalBalance, TransactionMetricResponse Income, TransactionMetricResponse Expense);
}
