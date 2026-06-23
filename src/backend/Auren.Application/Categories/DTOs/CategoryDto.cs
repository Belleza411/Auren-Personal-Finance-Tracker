using Auren.Domain.Enums;

namespace Auren.Application.Categories.DTOs
{
	public sealed record CategoryDto(
        string Name,
        TransactionType TransactionType
    );
}
