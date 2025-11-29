namespace Auren.Domain.Entities
{
	public class RefreshToken
	{
        public Guid RefreshTokenId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }
        public DateTime RevokedAt { get; set; }
        public string ReplacedByToken { get; set; } = string.Empty;
        public string ReasonRevoked { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
