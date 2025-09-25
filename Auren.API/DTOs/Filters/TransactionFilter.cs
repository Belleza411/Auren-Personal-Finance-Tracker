using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Filters
{
	public class TransactionFilter
	{
		public bool? IsIncome { get; set; }
		public bool? IsExpense { get; set; }
        public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public string? Category { get; set; }
		public string? PaymentMethod { get; set; }
    }
}
