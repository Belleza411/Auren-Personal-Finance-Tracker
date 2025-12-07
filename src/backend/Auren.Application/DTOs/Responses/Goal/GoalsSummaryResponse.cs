namespace Auren.Application.DTOs.Responses.Goal
{
	public sealed record GoalsSummaryResponse(
		int TotalGoals,
		int GoalsCompleted,
		int ActiveGoals
	);
}
