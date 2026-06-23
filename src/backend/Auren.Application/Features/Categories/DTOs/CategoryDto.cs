using Auren.Domain.Enums;

namespace Auren.Application.Features.Categories.DTOs
{
	public sealed record CategoryDto(
        string Name,
        TransactionType TransactionType
    );
}
