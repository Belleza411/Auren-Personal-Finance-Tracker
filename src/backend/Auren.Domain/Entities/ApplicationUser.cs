using Microsoft.AspNetCore.Identity;

namespace Auren.Domain.Entities
{
	public class ApplicationUser : IdentityUser<Guid>
	{
        public Guid UserId { 
            get => Id;
            set => Id = value; 
        }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string? Currency { get; set; } = "USD";
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
