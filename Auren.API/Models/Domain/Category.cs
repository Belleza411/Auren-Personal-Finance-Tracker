using Auren.API.Models.Enums;

namespace Auren.API.Models.Domain
{
	public class Category
	{
        public Guid CategoryId { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TransactionType TransactionType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
