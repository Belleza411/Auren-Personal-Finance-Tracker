using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Application.Features.Transactions.Queries.GetBalance;
using Auren.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Auren.Application.Features.Dashboard.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryHandler(IAppDbContext db)
    {
        public async Task<Result<DashboardSummaryResponse>> Handle(
            GetDashboardSummaryQuery query, CancellationToken ct)
        {
            var (startDate, endDate) = query.TimePeriod.GetTimePeriodRange();
            var (lastStartDate, lastEndDate) = query.TimePeriod.GetPreviousTimePeriodRange();

            var periodTotals = await db.Transactions
                .Where(t => t.UserId == query.UserId &&
                            t.TransactionDate >= lastStartDate &&
                            t.TransactionDate <= endDate)
                .GroupBy(t => new { t.TransactionType, IsCurrent = t.TransactionDate >= startDate })
                .Select(g => new { g.Key.TransactionType, g.Key.IsCurrent, Total = g.Sum(t => t.Amount) })
                .ToListAsync(ct);

            var currentIncome = periodTotals.FirstOrDefault(x => x.IsCurrent && x.TransactionType == TransactionType.Income)?.Total ?? 0;
            var currentExpense = periodTotals.FirstOrDefault(x => x.IsCurrent && x.TransactionType == TransactionType.Expense)?.Total ?? 0;
            var lastIncome = periodTotals.FirstOrDefault(x => !x.IsCurrent && x.TransactionType == TransactionType.Income)?.Total ?? 0;
            var lastExpense = periodTotals.FirstOrDefault(x => !x.IsCurrent && x.TransactionType == TransactionType.Expense)?.Total ?? 0;

            var balance = await GetBalance(query.UserId, ct);
            var lastMonthBalance = lastIncome - lastExpense;

            var currentAvgDaily = AverageDailySpending(currentExpense, startDate, endDate);
            var lastAvgDaily = AverageDailySpending(lastExpense, lastStartDate, lastEndDate);

            return Result.Success(new DashboardSummaryResponse(
                TotalBalance: new TransactionMetricResponse(
                    Amount: balance.Balance,
                    PercentageChange: PercentageChange(balance.Balance, lastMonthBalance)),
                Income: new TransactionMetricResponse(
                    Amount: currentIncome,
                    PercentageChange: PercentageChange(currentIncome, lastIncome)),
                Expense: new TransactionMetricResponse(
                    Amount: currentExpense,
                    PercentageChange: PercentageChange(currentExpense, lastExpense)),
                AverageDailySpending: new TransactionMetricResponse(
                    Amount: currentAvgDaily,
                    PercentageChange: PercentageChange(currentAvgDaily, lastAvgDaily))
            ));
        }

        public async Task<BalanceSummaryResponse> GetBalance(
            Guid userId,
            CancellationToken ct)
        {
            var totals = await db.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.TransactionType)
                .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
                .ToListAsync(ct);

            var income = totals.FirstOrDefault(x => x.Type == TransactionType.Income)?.Total ?? 0;
            var expense = totals.FirstOrDefault(x => x.Type == TransactionType.Expense)?.Total ?? 0;

            return new BalanceSummaryResponse(
                Income: income,
                Expense: expense,
                Balance: income - expense
            );
        }

        private static decimal PercentageChange(decimal current, decimal previous)
        {
            if (previous == 0) return current == 0 ? 0 : 100;
            var change = ((current - previous) / Math.Abs(previous)) * 100;
            return Math.Round(Math.Clamp(change, -100, 100), 1, MidpointRounding.AwayFromZero);
        }

        private static decimal AverageDailySpending(decimal expense, DateTime start, DateTime end)
        {
            var days = (end - start).TotalDays + 1;
            return Math.Round(days > 0 ? expense / (decimal)days : 0, 2);
        }
    }
}