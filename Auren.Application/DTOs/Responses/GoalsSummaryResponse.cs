namespace Auren.Application.DTOs.Responses
{
	public sealed record GoalsSummaryResponse(
		int TotalGoals,
		int GoalsCompleted,
		int ActiveGoals
	);
}
