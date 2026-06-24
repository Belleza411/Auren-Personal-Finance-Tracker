using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Transactions.Queries.GetTransactionById
{
    public record GetTransactionByIdQuery(Guid TransactionId, Guid UserId);
}
