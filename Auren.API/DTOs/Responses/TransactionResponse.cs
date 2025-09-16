using Auren.API.Models.Domain;
using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Responses
{
	public sealed record TransactionResponse(
		Guid TransactionId,
		Guid UserId,
        string Name,
        decimal Amount,
        Category Category,
		TransactionType TransactionType,	
		PaymentType PaymentType,
		DateTime TransactionDate,
		DateTime CreatedAt
    );
}
