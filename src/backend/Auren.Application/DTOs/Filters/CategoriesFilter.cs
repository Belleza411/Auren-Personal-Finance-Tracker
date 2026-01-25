using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Filters
{
	public class CategoriesFilter
	{
        public string? SearchTerm { get; set; }
        public TransactionType? TransactionType { get; set; }
        public IEnumerable<string>? Categories { get; set; }
    }
}
