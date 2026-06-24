using Auren.Application.Features.Transactions.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Transactions.Commands.UpdateTransaction
{
    public record UpdateTransactionCommand(Guid UserId, Guid TransactionId, TransactionDto Dto);
}
