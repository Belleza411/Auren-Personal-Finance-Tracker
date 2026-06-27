using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Transactions.Queries.GetTransactionById
{
    public class GetTransactionByIdHandler(IAppDbContext db)
    {
        public async Task<Result<Transaction>> Handle(
            GetTransactionByIdQuery query,
            CancellationToken ct)
        {
            var transaction = await db.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t =>
                    t.Id == query.TransactionId &&
                    t.UserId == query.UserId,
                    ct);

            return transaction != null
                ? Result.Success(transaction)
                : Result.Failure<Transaction>(Error.NotFound("Transaction not found"));
        }
    }
}
