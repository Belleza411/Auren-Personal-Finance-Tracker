using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Transactions.Commands.DeleteTransaction
{
    public record DeleteTransactionCommand(Guid UserId, Guid TransactionId);
}
