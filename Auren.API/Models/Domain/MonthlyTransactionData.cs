using Auren.API.Models.Enums;

namespace Auren.API.Models.Domain
{
	public class MonthlyTransactionData
	{
        public DateTime Year { get; set; }
        public DateTime Month { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Total { get; set; }
    }
}
