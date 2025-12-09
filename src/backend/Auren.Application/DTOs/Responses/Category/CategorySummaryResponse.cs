namespace Auren.Application.DTOs.Responses.Category
{
	public sealed record CategorySummaryResponse(int TotalCategories, string MostUsedCategory, string HighestSpendingCategory);
}
