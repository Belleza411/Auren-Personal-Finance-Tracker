using Auren.Application.Features.Transactions.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Transactions.Queries.GetTransactions
{
    public record GetTransactionsQuery(
        Guid UserId,
        TransactionFilter Filter,
        int PageNumber,
        int PageSize);
}
