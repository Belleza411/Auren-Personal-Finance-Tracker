namespace Auren.API.DTOs.Responses
{
	public sealed record CategorySummaryResponse(int totalCategories, string mostUsedCategory, string highestSpendingCategory);
}
