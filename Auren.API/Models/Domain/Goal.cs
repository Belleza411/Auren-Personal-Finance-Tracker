using Auren.API.Models.Enums;

namespace Auren.API.Models.Domain
{
	public class Goal
	{
        public Guid GoalId { get; set; }
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
