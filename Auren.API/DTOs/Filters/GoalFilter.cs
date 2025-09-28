namespace Auren.API.DTOs.Filters
{
	public class GoalFilter
	{
		public bool? IsCompleted { get; set; }
		public bool? IsOnTrack { get; set; }
		public bool? IsOnHold { get; set; }
		public bool? IsNotStarted { get; set; }
		public bool? IsBehindSchedule { get; set; }
		public bool? IsCancelled { get; set; }
    }
}
