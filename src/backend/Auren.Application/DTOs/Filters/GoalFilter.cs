using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Filters
{
	public class GoalFilter
	{
		public string? SearchTerm { get; set; }
        public GoalStatus? GoalStatus { get; set; }
    }
}
