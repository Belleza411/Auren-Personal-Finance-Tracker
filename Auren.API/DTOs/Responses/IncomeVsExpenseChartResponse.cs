using Auren.API.Models.Domain;

namespace Auren.API.DTOs.Responses
{
	public sealed record IncomeVsExpenseChartResponse(
		List<MonthlyDataPoint> DataPoints,
		decimal TotalIncome,
		decimal TotalExpense
	);
}
