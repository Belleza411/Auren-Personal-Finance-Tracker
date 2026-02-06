using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Filters
{
	public class GoalFilter
	{
		public string? SearchTerm { get; set; }
        public GoalStatus? GoalStatus { get; set; }
		public decimal? MinBudget { get; set; }
		public decimal? MaxBudget { get; set; }
        public DateTime? TargetFrom { get; set; }
        public DateTime? TargetTo { get; set; }

    }
}
