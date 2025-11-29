namespace Auren.Application.DTOs.Responses
{
	public class GoalsOverviewResponse
	{
        public int ActiveGoals { get; set; }
        public int Completed { get; set; }
        public int OnTrack { get; set; }
        public int OnHold { get; set; }
        public int NotStarted { get; set; }
        public int BehindSchedule { get; set; }
        public int Cancelled { get; set; }
    }
}
