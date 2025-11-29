using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Responses
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
