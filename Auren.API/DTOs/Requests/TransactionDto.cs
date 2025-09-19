using Auren.API.Models.Domain;
using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Requests
{
	public sealed record TransactionDto(
        string Name,
        decimal Amount,
        Category Category,
        TransactionType TransactionType,
        PaymentType PaymentType
    );
}
