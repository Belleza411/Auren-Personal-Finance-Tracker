using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Application.Features.Dashboard.Helper;
using Auren.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Auren.Application.Features.Dashboard.Queries.GetExpenseBreakdown
{
    public class GetExpenseBreakdownHandler(IAppDbContext db)
    {
        public async Task<Result<ExpenseBreakdownResponse>> Handle(
            GetExpenseBreakdownQuery query,
            CancellationToken ct)
        {
            var (startDate, endDate) = query.TimePeriod.GetTimePeriodRange();

            var data = await db.Transactions
                .Where(t =>
                    t.UserId == query.UserId &&
                    t.TransactionType == TransactionType.Expense &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key.Name,
                    Total = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Total)
                .AsNoTracking()
                .ToListAsync(ct);

            var totalSpent = Math.Round(data.Sum(c => c.Total), 2);

            var labels = data.Select(c => c.Category).ToList();

            var amounts = data
                .Select(x => Math.Round(x.Total, 2))
                .ToList();

            var percentages = data
                .Select(x => totalSpent > 0
                    ? Math.Round((x.Total / totalSpent) * 100, 1, MidpointRounding.AwayFromZero)
                    : 0)
                .ToList();

            var backgroundColors = percentages
                .Select(p => ChartColorHelper.GetColorFromPercent(p))
                .ToList();

            return Result.Success(new ExpenseBreakdownResponse(labels, amounts, percentages, backgroundColors, totalSpent));
        }
    }
}
