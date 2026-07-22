namespace Auren.Application.Features.Dashboard.DTOs
{
	public sealed record ExpenseBreakdownResponse(
		IEnumerable<string> Labels,
		IEnumerable<decimal> Amounts,
		IEnumerable<decimal> Percentages,
		IEnumerable<string> BackgroundColors,
		decimal TotalSpent
    );
}
