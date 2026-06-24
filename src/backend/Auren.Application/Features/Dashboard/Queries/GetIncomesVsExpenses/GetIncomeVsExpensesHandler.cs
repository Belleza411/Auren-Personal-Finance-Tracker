using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Dashboard.Queries.GetIncomesVsExpenses
{
    public class GetIncomesVsExpensesHandler(IAppDbContext db)
    {
        private sealed record AggregatedData(DateTime? Date, TransactionType TransactionType, decimal Total);

        public async Task<Result<IncomesVsExpenseResponse>> Handle(
            GetIncomesVsExpensesQuery query,
            CancellationToken ct)
        {
            var (startDate, endDate) = query.TimePeriod.GetTimePeriodRange();

            var rawData = await db.Transactions
                .Where(t =>
                    t.UserId == query.UserId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .GroupBy(t => new { t.TransactionDate.Date, t.TransactionType })
                .Select(g => new AggregatedData(
                    g.Key.Date,
                    g.Key.TransactionType,
                    g.Sum(t => t.Amount)))
                .ToListAsync(ct);

            bool isDailyHybrid = query.TimePeriod is TimePeriod.ThisMonth or TimePeriod.LastMonth;

            var response = isDailyHybrid
                ? BuildDailyData(rawData, endDate)
                : BuildMonthlyData(rawData);

            return Result.Success(response);
        }

        private static IncomesVsExpenseResponse BuildDailyData(
            IEnumerable<AggregatedData> data,
            DateTime referenceDate)
        {
            var firstDay = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var days = new SortedSet<DateTime> { firstDay, lastDay };

            foreach (var d in data.Select(x => x.Date).Where(d => d.HasValue).Select(d => d!.Value.Date).Distinct())
                days.Add(d);

            var labels = new List<string>();
            var incomes = new List<decimal>();
            var expenses = new List<decimal>();

            foreach (var day in days)
            {
                labels.Add(day.ToString("MMM d"));
                incomes.Add(data
                    .Where(x => x.Date == day && x.TransactionType == TransactionType.Income)
                    .Sum(x => x.Total));
                expenses.Add(data
                    .Where(x => x.Date == day && x.TransactionType == TransactionType.Expense)
                    .Sum(x => x.Total));
            }

            return new IncomesVsExpenseResponse(labels, incomes, expenses);
        }

        private static IncomesVsExpenseResponse BuildMonthlyData(IEnumerable<AggregatedData> data)
        {
            var labels = new List<string>();
            var incomes = new List<decimal>();
            var expenses = new List<decimal>();

            var groupedByMonth = data
                .GroupBy(x => new { x.Date!.Value.Year, x.Date!.Value.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month);

            foreach (var monthGroup in groupedByMonth)
            {
                var monthStart = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1);
                labels.Add(monthStart.ToString("MMM yyyy"));
                incomes.Add(monthGroup.Where(x => x.TransactionType == TransactionType.Income).Sum(x => x.Total));
                expenses.Add(monthGroup.Where(x => x.TransactionType == TransactionType.Expense).Sum(x => x.Total));
            }

            return new IncomesVsExpenseResponse(labels, incomes, expenses);
        }
    }
}
