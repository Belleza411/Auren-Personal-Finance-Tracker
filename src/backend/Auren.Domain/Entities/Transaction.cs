using Auren.Domain.Common;
using Auren.Domain.Enums;

namespace Auren.Domain.Entities
{
	public class Transaction : IEntity, IHasTransactionType, IHasName
	{
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Category Category { get; set; } = null!;
        public TransactionType TransactionType { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}