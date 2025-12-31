
using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Filters
{
	public class TransactionFilter
	{
		public string? SearchTerm { get; set; }
        public TransactionType? TransactionType { get; set; }
        public decimal? MinAmount { get; set; }
		public decimal? MaxAmount { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? StartDate { get; set; }
		public IEnumerable<string>? Category { get; set; }
		public PaymentType? PaymentType { get; set; }
    }
}
