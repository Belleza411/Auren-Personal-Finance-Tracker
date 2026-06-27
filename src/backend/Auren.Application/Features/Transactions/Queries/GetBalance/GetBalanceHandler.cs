using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Features.Transactions.Queries.GetBalance
{
    public class GetBalanceHandler(IAppDbContext db)
    {
        public async Task<Result<BalanceSummaryResponse>> Handle(
            GetBalanceQuery query,
            CancellationToken ct)
        {
            var totals = await db.Transactions
                .Where(t => t.UserId == query.UserId)
                .GroupBy(t => t.TransactionType)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
                .AsNoTracking()
                .ToListAsync(ct);

            var income = totals.FirstOrDefault(x => x.Type == TransactionType.Income)?.Total ?? 0;
            var expense = totals.FirstOrDefault(x => x.Type == TransactionType.Expense)?.Total ?? 0;

            return Result.Success(new BalanceSummaryResponse(
                Income: income,
                Expense: expense,
                Balance: income - expense
            ));
        }
    }
}
