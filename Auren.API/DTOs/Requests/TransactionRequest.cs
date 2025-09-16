using Auren.API.Models.Domain;
using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Requests
{
	public sealed record TransactionRequest(
        string Name,
        decimal Amount,
        string Category,
        TransactionType TransactionType,
        PaymentType PaymentType
    );
}
