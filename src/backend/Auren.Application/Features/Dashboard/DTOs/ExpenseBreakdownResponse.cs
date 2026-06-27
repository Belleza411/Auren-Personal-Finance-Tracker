namespace Auren.Application.Features.Dashboard.DTOs
{
	public sealed record ExpenseBreakdownResponse(
		IEnumerable<string> Labels,
		IEnumerable<decimal> Data,
		IEnumerable<decimal> Percentage,
		IEnumerable<string> BackgroundColor,
		decimal TotalSpent
    );
}
