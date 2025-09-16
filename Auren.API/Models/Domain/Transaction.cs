using Auren.API.Models.Enums;

namespace Auren.API.Models.Domain
{
	public class Transaction
	{
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
