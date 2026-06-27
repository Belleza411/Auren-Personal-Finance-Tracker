using Auren.Application.Features.Transactions.DTOs;

namespace Auren.Application.Features.Transactions.Queries.GetTransactions
{
    public record GetTransactionsQuery(
        Guid UserId,
        TransactionFilter Filter,
        int PageNumber,
        int PageSize);
}
