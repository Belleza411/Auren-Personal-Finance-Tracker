using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Responses
{
    public sealed record CategoryOverviewResponse(
		string Category,
		TransactionType TransactionType,
		decimal TotalSpending,
		decimal AverageSpending,
		int TransactionCount,
		DateTime? LastUsed
    );
}
