using System.ComponentModel.DataAnnotations.Schema;

namespace Auren.Domain.Entities
{
	public class ProfileUserImage
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
		public string Extension { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
		public string Path { get; set; } = string.Empty;
		public ApplicationUser User { get; set; } = null!;
    }
}
