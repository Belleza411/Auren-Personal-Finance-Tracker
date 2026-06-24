using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

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
                .Select(p => GetColorFromPercent(p))
                .ToList();

            return Result.Success(new ExpenseBreakdownResponse(labels, amounts, percentages, backgroundColors, totalSpent));
        }

        private static string GetColorFromPercent(decimal percent, double alpha = 1)
        {
            percent = Math.Clamp(percent, 0.0m, 100.0m);

            var r = (int)Math.Round(255 - (percent * 2.55m));
            var g = (int)Math.Round(percent * 2.55m);

            return $"rgba({r}, {g}, 0, {alpha.ToString(CultureInfo.InvariantCulture)})";
        }
    }
}
