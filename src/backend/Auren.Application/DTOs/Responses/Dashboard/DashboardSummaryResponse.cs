using Auren.Application.DTOs.Responses.Transaction;

namespace Auren.Application.DTOs.Responses.Dashboard
{
	public sealed record DashboardSummaryResponse(TransactionMetricResponse TotalBalance, TransactionMetricResponse Income, TransactionMetricResponse Expense);
}
