using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Requests
{
	public sealed record TransactionDto(
        string Name,
        decimal Amount,
        string Category,
        TransactionType TransactionType,
        PaymentType PaymentType
    );
}
