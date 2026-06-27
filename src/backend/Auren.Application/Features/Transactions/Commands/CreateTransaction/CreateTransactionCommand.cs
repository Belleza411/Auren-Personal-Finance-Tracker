using Auren.Application.Features.Transactions.DTOs;

namespace Auren.Application.Features.Transactions.Commands.CreateTransaction
{
    public record CreateTransactionCommand(
        Guid UserId,
        TransactionDto Dto  
    );
}
