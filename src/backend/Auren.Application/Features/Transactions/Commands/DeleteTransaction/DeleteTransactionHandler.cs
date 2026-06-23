using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Transactions.Commands.DeleteTransaction
{
    public class DeleteTransactionHandler(
        IAppDbContext db)
    {
        public async Task<Result<bool>> Handle(
            DeleteTransactionCommand cmd,
            CancellationToken ct)
        {
            var transaction = await db.Transactions
            .FirstOrDefaultAsync(t =>
                t.Id == cmd.TransactionId &&
                t.UserId == cmd.UserId, ct);

            if (transaction == null)
                return Result.Failure<bool>(Error.NotFound("Transaction not found."));

            db.Transactions.Remove(transaction);
            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(true)
                : Result.Failure<bool>(Error.DeleteFailed("Failed to delete transaction."));
        }
    }
}
