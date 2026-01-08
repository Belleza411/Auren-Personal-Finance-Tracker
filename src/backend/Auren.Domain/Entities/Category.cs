using Auren.Domain.Common;
using Auren.Domain.Enums;

namespace Auren.Domain.Entities
{
	public class Category : IEntity, IHasTransactionType
	{
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
