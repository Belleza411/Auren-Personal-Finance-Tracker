using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Responses
{
	public sealed record GoalResponse(
		Guid GoalId,
		Guid UserId,
		string Name,
		string Description,
		decimal Spent,
		decimal Budget,
		GoalStatus Status,
		DateTime TargetDate,
		DateTime CreatedAt
	);
}
