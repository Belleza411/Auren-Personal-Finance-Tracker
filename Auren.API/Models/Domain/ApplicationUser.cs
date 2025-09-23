using Microsoft.AspNetCore.Identity;

namespace Auren.API.Models.Domain
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
        public bool IsGoogleUser { get; set; }
        public string? Currency { get; set; } = "USD";
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();

    }
}
