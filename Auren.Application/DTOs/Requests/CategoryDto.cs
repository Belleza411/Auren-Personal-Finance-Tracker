using Auren.Domain.Enums;

namespace Auren.Application.DTOs.Requests
{
	public sealed record CategoryDto(
        string Name,
        TransactionType TransactionType
    );
}
