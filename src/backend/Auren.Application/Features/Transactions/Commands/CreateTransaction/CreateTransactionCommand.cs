using Auren.Application.Features.Transactions.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Transactions.Commands.CreateTransaction
{
    public record CreateTransactionCommand(
        Guid UserId,
        TransactionDto Dto  
    );
}
