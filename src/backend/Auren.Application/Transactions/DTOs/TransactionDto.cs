using Auren.Domain.Enums;

namespace Auren.Application.Transactions.DTOs
{
	public sealed record TransactionDto(
        string Name,
        decimal Amount,
        string Category,
        TransactionType TransactionType,
        PaymentType PaymentType,
        DateTime TransactionDate
    );
}
