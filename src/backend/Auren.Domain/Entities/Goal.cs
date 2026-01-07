using Auren.Domain.Common;
using Auren.Domain.Enums;

namespace Auren.Domain.Entities
{
	public class Goal : IEntity
	{
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Spent { get; set; } = 0;
        public decimal Budget { get; set; }
        public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
        public int? CompletionPercentage { get; set; } = 0;
        public string? TimeRemaining { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime TargetDate { get; set; }
    }
}
