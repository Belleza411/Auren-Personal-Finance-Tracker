using Auren.Application.DTOs.Responses.Dashboard;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            var (startDate, endDate) = timePeriod.GetTimePeriodRange();
            var (lastStartDate, lastEndDate) = timePeriod.GetPreviousTimePeriodRange();

            var (currentIncome, currentExpense) = await transactionRepository.GetMonthlyTotalsAsync(userId, startDate, endDate, cancellationToken);
            var (lastMonthIncome, lastMonthExpense) = await transactionRepository.GetMonthlyTotalsAsync(userId, lastStartDate, lastEndDate, cancellationToken);
            
            var totalBalance = await transactionRepository.GetBalanceAsync(userId, cancellationToken);
            var lastMonthBalance = lastMonthIncome - lastMonthExpense;

            var balanceChange = CalculatePercentageChange(totalBalance.Balance, lastMonthBalance);
            var incomeChange = CalculatePercentageChange(currentIncome, lastMonthIncome);
            var expenseChange = CalculatePercentageChange(currentExpense, lastMonthExpense);

            var currentTotalDays = (endDate - startDate).TotalDays + 1;
            var lastMonthTotalDays = (lastEndDate - lastStartDate).TotalDays + 1;
            var currentAvgDailySpending = Math.Round(currentTotalDays > 0 ? currentExpense / (decimal)currentTotalDays : 0, 2);
            var lastMonthAvgDailySpending = Math.Round(lastMonthTotalDays > 0 ? lastMonthExpense / (decimal)lastMonthTotalDays : 0, 2);
            var avgDailySpendingChange = CalculatePercentageChange(currentAvgDailySpending, lastMonthAvgDailySpending);

            return new DashboardSummaryResponse(
                TotalBalance: new TransactionMetricResponse(
                    Amount: totalBalance.Balance,
                    PercentageChange: balanceChange
                ),
                Income: new TransactionMetricResponse(
                    Amount: currentIncome,
                    PercentageChange: incomeChange
                ),
                Expense: new TransactionMetricResponse(
                    Amount: currentExpense,
                    PercentageChange: expenseChange
                ),
                AverageDailySpending: new TransactionMetricResponse(
                    Amount: currentAvgDailySpending,
                    PercentageChange: avgDailySpendingChange
                )
            );
        }

        public async Task<IncomesVsExpenseResponse> GetIncomesVsExpensesAsync(
            Guid userId,
            TimePeriod? timePeriod,
            CancellationToken cancellationToken)
        {
            var (startDate, endDate) = timePeriod.GetTimePeriodRange();

            bool isDailyHybrid = timePeriod is TimePeriod.ThisMonth or TimePeriod.LastMonth;

            var rawData = await dbContext.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate <= endDate)
                .GroupBy(t => new
                {
                    t.TransactionDate.Date,
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
            var (startDate, endDate) = timePeriod.GetTimePeriodRange();

            var data = await dbContext.Transactions
                .Where(t =>
                    t.UserId == userId &&
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
                .ToListAsync(cancellationToken);


            var totalSpent = Math.Round(data.Sum(x => x.Total), 2);

            var labels = data.Select(x => x.Category).ToList();

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

            return new ExpenseBreakdownResponse(labels, amounts, percentages, backgroundColors, totalSpent);
        }


        private static decimal CalculatePercentageChange(decimal current, decimal previous)
        {
            if (previous == 0) return current == 0 ? 0 : 100;

            var change = ((current - previous) / Math.Abs(previous)) * 100;

            return Math.Round(Math.Clamp(change, -100, 100), 1, MidpointRounding.AwayFromZero);
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
                incomes.Add(monthGroup.Where(x => x.TransactionType == TransactionType.Income).Sum(x => x.Total));
                expenses.Add(monthGroup.Where(x => x.TransactionType == TransactionType.Expense).Sum(x => x.Total));

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

        private static string GetColorFromPercent(decimal percent, double alpha = 1)
        {
            percent = Math.Clamp(percent, 0.0m, 100.0m);

            var r = (int)Math.Round(255 - (percent * 2.55m));
            var g = (int)Math.Round(percent * 2.55m);

            return $"rgba({r}, {g}, 0, {alpha.ToString(CultureInfo.InvariantCulture)})";
        }

    }
}
