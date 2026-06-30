using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Application.Features.Dashboard.Helper;
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
                .AsNoTracking()
                .ToListAsync(ct);

            var currentIncome = periodTotals.FirstOrDefault(x => x.IsCurrent && x.TransactionType == TransactionType.Income)?.Total ?? 0;
            var currentExpense = periodTotals.FirstOrDefault(x => x.IsCurrent && x.TransactionType == TransactionType.Expense)?.Total ?? 0;
            var lastIncome = periodTotals.FirstOrDefault(x => !x.IsCurrent && x.TransactionType == TransactionType.Income)?.Total ?? 0;
            var lastExpense = periodTotals.FirstOrDefault(x => !x.IsCurrent && x.TransactionType == TransactionType.Expense)?.Total ?? 0;

            var balance = await GetBalance(query.UserId, ct);
            var lastMonthBalance = lastIncome - lastExpense;

            var currentAvgDaily = DashboardCalculatorHelper.AverageDailySpending(currentExpense, startDate, endDate);
            var lastAvgDaily = DashboardCalculatorHelper.AverageDailySpending(lastExpense, lastStartDate, lastEndDate);

            return Result.Success(new DashboardSummaryResponse(
                TotalBalance: new TransactionMetricResponse(
                    Amount: balance.Balance,
                    PercentageChange: DashboardCalculatorHelper.PercentageChange(balance.Balance, lastMonthBalance)),
                Income: new TransactionMetricResponse(
                    Amount: currentIncome,
                    PercentageChange: DashboardCalculatorHelper.PercentageChange(currentIncome, lastIncome)),
                Expense: new TransactionMetricResponse(
                    Amount: currentExpense,
                    PercentageChange: DashboardCalculatorHelper.PercentageChange(currentExpense, lastExpense)),
                AverageDailySpending: new TransactionMetricResponse(
                    Amount: currentAvgDaily,
                    PercentageChange: DashboardCalculatorHelper.PercentageChange(currentAvgDaily, lastAvgDaily))
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
    }
}