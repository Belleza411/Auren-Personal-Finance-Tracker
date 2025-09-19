using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Requests
{
	public sealed record CategoryDto(
        string Name,
        TransactionType TransactionType
    );
}
