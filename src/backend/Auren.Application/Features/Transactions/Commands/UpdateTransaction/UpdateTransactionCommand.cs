using Auren.Application.Features.Transactions.DTOs;

namespace Auren.Application.Features.Transactions.Commands.UpdateTransaction
{
    public record UpdateTransactionCommand(Guid UserId, Guid TransactionId, TransactionDto Dto);
}
