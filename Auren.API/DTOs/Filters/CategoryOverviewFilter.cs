namespace Auren.API.DTOs.Filters
{
	public class CategoryOverviewFilter
	{
		public string? Category { get; set; } = string.Empty;
		public double? MinAmount { get; set; }
		public double? MaxAmount{ get; set; }
		public int? MinTransactionCount { get; set; }
		public int? MaxTransactionCount { get; set; }
    }
}
