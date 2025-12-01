namespace Auren.Application.DTOs.Responses.Category
{
	public sealed record CategorySummaryResponse(int totalCategories, string mostUsedCategory, string highestSpendingCategory);
}
