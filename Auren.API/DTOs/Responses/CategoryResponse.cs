using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Responses
{
	public sealed record CategoryResponse(
		Guid CategoryId,
		Guid UserId,
		string Name,
		TransactionType TransactionType,
		DateTime CreatedAt
	);
}
