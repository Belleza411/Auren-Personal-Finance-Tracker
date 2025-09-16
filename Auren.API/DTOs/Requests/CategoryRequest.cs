using Auren.API.Models.Enums;

namespace Auren.API.DTOs.Requests
{
	public sealed record CategoryRequest(
        string Name,
        TransactionType TransactionType
    );
}
