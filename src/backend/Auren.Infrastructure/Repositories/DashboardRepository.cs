using Auren.Application.DTOs.Responses.Dashboard;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Auren.Infrastructure.Repositories
{
    public class DashboardRepository(AurenDbContext dbContext, ITransactionRepository transactionRepository) : IDashboardRepository
    {
        private sealed record AggregatedData(DateTime? Date, TransactionType TransactionType, decimal Total);
        public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid userId, TimePeriod? timePeriod, CancellationToken cancellationToken)
        {
            var (startDate, endDate) = GetTimePeriodRange(timePeriod);

            var monthlyData = await dbContext.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= endDate)
                .GroupBy(t => new
                {
                    IsCurrentMonth = t.TransactionDate >= startDate,
                    t.TransactionType
                })
                .Select(g => new
                {
                    g.Key.IsCurrentMonth,
                    g.Key.TransactionType,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync(cancellationToken);

            var currentIncome = monthlyData
                .FirstOrDefault(x => x.IsCurrentMonth && x.TransactionType == TransactionType.Income)?.Total ?? 0;

            var lastMonthIncome = monthlyData
                .FirstOrDefault(x => !x.IsCurrentMonth && x.TransactionType == TransactionType.Income)?.Total ?? 0;

            var currentExpense = monthlyData
                .FirstOrDefault(x => x.IsCurrentMonth && x.TransactionType == TransactionType.Expense)?.Total ?? 0;

            var lastMonthExpense = monthlyData
                .FirstOrDefault(x => !x.IsCurrentMonth && x.TransactionType == TransactionType.Expense)?.Total ?? 0;

            var currentBalance = await transactionRepository.GetBalanceAsync(userId, startDate, endDate, cancellationToken);
            var lastMonthBalance = await transactionRepository.GetBalanceAsync(userId, startDate, endDate, cancellationToken);

            var balanceChange = CalculatePercentageChange(currentBalance.Balance, lastMonthBalance.Balance);
            var incomeChange = CalculatePercentageChange(currentIncome, lastMonthIncome);
            var expenseChange = CalculatePercentageChange(currentExpense, lastMonthExpense);

            return new DashboardSummaryResponse(
                TotalBalance: new TransactionMetricResponse(
                    Amount: currentBalance.Balance,
                    PercentageChange: balanceChange
                ),
                Income: new TransactionMetricResponse(
                    Amount: currentIncome,
                    PercentageChange: incomeChange
                ),
                Expense: new TransactionMetricResponse(
                    Amount: currentExpense,
                    PercentageChange: expenseChange
                )
            );
        }

        public async Task<IncomesVsExpenseResponse> GetIncomesVsExpensesAsync(
            Guid userId,
            TimePeriod? timePeriod,
            CancellationToken cancellationToken)
        {
            var (startDate, endDate) = GetTimePeriodRange(timePeriod);

            bool isDailyHybrid = timePeriod is TimePeriod.ThisMonth or TimePeriod.LastMonth;

            var rawData = await dbContext.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .GroupBy(t => new
                {
                    Date = t.TransactionDate.Date,
                    t.TransactionType
                })
                .Select(g => new AggregatedData(
                    g.Key.Date,
                    g.Key.TransactionType,
                    g.Sum(t => t.Amount))
                )
                .ToListAsync(cancellationToken);

            return isDailyHybrid
                ? BuildDailyHybridData(rawData, endDate)
                : BuildMonthlyHybridData(rawData);
        }

        public async Task<ExpenseBreakdownResponse> GetExpenseBreakdownAsync(
            Guid userId,
            TimePeriod? timePeriod,
            CancellationToken cancellationToken)
        {
            var (startDate, endDate) = GetTimePeriodRange(timePeriod);

            var data = await dbContext.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionType == TransactionType.Expense &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .GroupBy(t => t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync(cancellationToken);


            var totalSpent = data.Sum(x => Math.Round(x.Total, 2));
            var labels = data.Select(x => x.Category.Name).ToList();
            var amounts = data.Select(x => x.Total).ToList();
            var percentages = data.Select(x => totalSpent > 0 ? Math.Round((Math.Round(x.Total, 2) / totalSpent) * 100, 2) : 0).ToList();
            var backgroundColors = percentages.Select(p => GetColorFromPercent(p)).ToList();

            return new ExpenseBreakdownResponse(labels, amounts, percentages, backgroundColors, totalSpent);
        }


        private static decimal CalculatePercentageChange(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            var change = ((current - previous) / previous) * 100;

            return Math.Round(change, 2);
        }

        private static IncomesVsExpenseResponse BuildDailyHybridData(
            IEnumerable<AggregatedData> data,
            DateTime referenceDate)
        {
            var firstDay = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var activeDays = data
                .Select(x => x.Date)
                .Where(d => d.HasValue)
                .Select(d => d!.Value.Date)
                .Distinct();

            var days = new SortedSet<DateTime> { firstDay, lastDay };
            foreach (var d in activeDays)
                days.Add(d);


            List<string> labels = [];
            List<decimal> incomes = [];
            List<decimal> expenses = [];

            foreach (var day in days)
            {
                labels.Add(day.ToString("MMM d"));

                incomes.Add(
                    data.Where(x => x.Date == day && x.TransactionType == TransactionType.Income)
                        .Sum(x => x.Total)
                );

                expenses.Add(
                    data.Where(x => x.Date == day && x.TransactionType == TransactionType.Expense)
                        .Sum(x => x.Total)
                );
            }

            return new IncomesVsExpenseResponse(labels, incomes, expenses);
        }

        private static IncomesVsExpenseResponse BuildMonthlyHybridData(IEnumerable<AggregatedData> data)
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

                labels.Add(monthStart.ToString("MMM"));
                incomes.Add(0);
                expenses.Add(0);

                var activeDays = monthGroup
                    .Select(x => x.Date!.Value)
                    .Distinct()
                    .OrderBy(d => d);

                foreach (var day in activeDays)
                {
                    labels.Add(day.ToString("MMM d"));

                    incomes.Add(
                        data.Where(x => x.Date == day && x.TransactionType == TransactionType.Income)
                            .Sum(x => x.Total)
                    );

                    expenses.Add(
                        data.Where(x => x.Date == day && x.TransactionType == TransactionType.Expense)
                            .Sum(x => x.Total)
                    );
                }
            }

            return new IncomesVsExpenseResponse(labels, incomes, expenses);
        }

        private static (DateTime start, DateTime end) GetTimePeriodRange(TimePeriod? timePeriod)
        {
            return timePeriod switch
            {
                TimePeriod.Last3Months => DateTime.Today.GetLast3MonthRange(),
                TimePeriod.Last6Months => DateTime.Today.GetLast6MonthRange(),
                TimePeriod.ThisYear => DateTime.Today.GetThisYearRange(),
                TimePeriod.LastMonth => DateTime.Today.GetLastMonthRange(),
                TimePeriod.ThisMonth => DateTime.Today.GetCurrentMonthRange(),
                TimePeriod.AllTime => (DateTime.MinValue, DateTime.Today),
                _ => (DateTime.MinValue, DateTime.Today)
            };
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
