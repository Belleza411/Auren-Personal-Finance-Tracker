using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Requests
{
	public sealed record GoalDto(
        string Name,
        string Description,
        decimal Spent,
        decimal Budget,
        GoalStatus Status,
        DateTime TargetDate
    );
}
