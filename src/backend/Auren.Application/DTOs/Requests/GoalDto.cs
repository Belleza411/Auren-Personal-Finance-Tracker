using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Requests
{
	public sealed record GoalDto(
        string Name,
        string Description,
        string Emoji,
        decimal Spent,
        decimal Budget,
        GoalStatus Status,
        DateTime TargetDate
    );
}
