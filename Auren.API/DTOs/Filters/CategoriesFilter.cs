namespace Auren.API.DTOs.Filters
{
	public class CategoriesFilter
	{
        public bool? IsIncome { get; set; }
        public bool? IsExpense { get; set; }
        public int Transactions { get; set; }
        public string? Category { get; set; }
    }
}
