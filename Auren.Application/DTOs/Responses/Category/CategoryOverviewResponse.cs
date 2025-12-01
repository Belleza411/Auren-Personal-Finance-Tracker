using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Responses.Category
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
