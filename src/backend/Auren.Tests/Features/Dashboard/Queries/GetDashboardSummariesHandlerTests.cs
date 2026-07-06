using Auren.Application.Features.Dashboard.Queries.GetDashboardSummary;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using Auren.Tests.Features.Dashboard.Helpers;
using FluentAssertions;

namespace Auren.Tests.Features.Dashboard.Queries
{
    public class GetDashboardSummariesHandlerTests
    {
        private static readonly DateTime ReferenceDate = new(2026, 7, 15);
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetDashboardSummaryHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();
        private Category _incomeCategory = null!;
        private Category _expenseCategory = null!;

        public GetDashboardSummariesHandlerTests()
        {
            var dateTime = new FakeDateTimeProvider(ReferenceDate);
            _handler = new GetDashboardSummaryHandler(_db, dateTime);
        }

        private async Task SetupCategoriesAsync(CancellationToken ct)
        {
            (_incomeCategory, _expenseCategory) =
                await DashboardSeedHelper.SeedCategoriesAsync(_db, _userId, ct);
        }

        [Fact]
        public async Task Handle_NoTransactions_ReturnsNoBalance()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            var result = await _handler.Handle(new GetDashboardSummaryQuery(
                UserId: _userId,
                TimePeriod: TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalBalance.Amount.Should().Be(0);
            result.Value.Income.Amount.Should().Be(0);
            result.Value.Expense.Amount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_MultipleTransactions_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            // ReferenceDate = July 15, 2026
            // ThisMonth current  = July 1  → July 31
            // ThisMonth previous = June 1  → June 30

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, _userId, _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                (200, ReferenceDate.AddDays(-1)),  // July 14
                (100, ReferenceDate.AddDays(-3)),  // July 12 
                (100, ReferenceDate.AddDays(-2)),  // July 13 
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, _userId, _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                (50,  ReferenceDate.AddDays(-2)),  // July 13 
                (75,  ReferenceDate.AddDays(-1)),  // July 14 
                (10,  ReferenceDate.AddDays(-3)),  // July 12
                }, ct);

            // Current:  Income 400, Expense 135, Net 265
            // Previous: Income 0,   Expense 0   (nothing seeded in June)
            // July has 31 days → AvgDaily = 135 / 31 = 4.35

            var result = await _handler.Handle(new GetDashboardSummaryQuery(
                UserId: _userId,
                TimePeriod: TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalBalance.Amount.Should().Be(265);
            result.Value.TotalBalance.PercentageChange.Should().Be(100);
            result.Value.Income.Amount.Should().Be(400);
            result.Value.Income.PercentageChange.Should().Be(100);
            result.Value.Expense.Amount.Should().Be(135);
            result.Value.Expense.PercentageChange.Should().Be(100);
            result.Value.AverageDailySpending.Amount.Should().Be(9m); // 135/15
            result.Value.AverageDailySpending.PercentageChange.Should().Be(100);
        }

        [Fact]
        public async Task Handle_TimePeriodThisMonth_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            // ReferenceDate = July 15, 2026
            // ThisMonth current  = July 1  → July 31
            // ThisMonth previous = June 1  → June 30

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, _userId, _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                (1000, ReferenceDate.AddDays(-1)),  // July 14
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, _userId, _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                (200, ReferenceDate.AddDays(-2)),   // July 13 
                (50,  ReferenceDate.AddDays(-1)),   // July 14 
                (20,  ReferenceDate.AddDays(-3)),   // July 12
                }, ct);

            // Current:  Income 1000, Expense 270, Net 730
            // Previous: Income 0, Expense 0
            // Reference Date is July 15 → AvgDaily = 270 / 15 = 18

            var result = await _handler.Handle(new GetDashboardSummaryQuery(
                UserId: _userId,
                TimePeriod: TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalBalance.Amount.Should().Be(730);
            result.Value.TotalBalance.PercentageChange.Should().Be(100);
            result.Value.Income.Amount.Should().Be(1000);
            result.Value.Income.PercentageChange.Should().Be(100);
            result.Value.Expense.Amount.Should().Be(270);
            result.Value.Expense.PercentageChange.Should().Be(100);
            result.Value.AverageDailySpending.Amount.Should().Be(18m); // 270/15
            result.Value.AverageDailySpending.PercentageChange.Should().Be(100);
        }

        [Fact]
        public async Task Handle_TimePeriodLastMonth_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            // ReferenceDate = July 15, 2026
            // LastMonth current  = June 1  → June 30  (30 days)
            // LastMonth previous = May 1   → May 31   (31 days)

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, _userId, _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                (1000, ReferenceDate.AddMonths(-1).AddDays(-1)),  // June 14 
                (500,  ReferenceDate.AddMonths(-2).AddDays(-1)),  // May 14  
                (500,  ReferenceDate.AddMonths(-2).AddDays(-3)),  // May 12  
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, _userId, _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                (100, ReferenceDate.AddMonths(-1).AddDays(-2)),   // June 13 
                (100, ReferenceDate.AddMonths(-1).AddDays(-3)),   // June 12 
                (200, ReferenceDate.AddMonths(-2).AddDays(-2)),   // May 13  
                (50,  ReferenceDate.AddMonths(-2).AddDays(-1)),   // May 14  
                (20,  ReferenceDate.AddMonths(-2).AddDays(-3)),   // May 12 
                }, ct);

            // Current (June):  Income 1000, Expense 200, Net 800
            // Previous (May):  Income 1000, Expense 270, Net 730
            // BalanceChange    = (800 - 730) / 730 * 100 = 9.6%
            // IncomeChange     = (1000 - 1000) / 1000 * 100 = 0%
            // ExpenseChange    = (200 - 270) / 270 * 100 = -25.9%
            // AvgDaily current = 200 / 30 = 6.67 (June has 30 days)
            // AvgDaily prev    = 270 / 31 = 8.71 (May has 31 days)
            // AvgDailyChange   = (6.67 - 8.71) / 8.71 * 100 = -23.4%

            var result = await _handler.Handle(
                new GetDashboardSummaryQuery(_userId, TimePeriod.LastMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalBalance.Amount.Should().Be(1530);       // all-time: 2000 - 470
            result.Value.TotalBalance.PercentageChange.Should().Be(9.6m);
            result.Value.Income.Amount.Should().Be(1000);
            result.Value.Income.PercentageChange.Should().Be(0);
            result.Value.Expense.Amount.Should().Be(200);
            result.Value.Expense.PercentageChange.Should().Be(-25.9m);
            result.Value.AverageDailySpending.Amount.Should().Be(6.67m);
            result.Value.AverageDailySpending.PercentageChange.Should().Be(-23.4m);
        }

        [Fact]
        public async Task Handle_TimePeriodLast3Months_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            // ReferenceDate = July 15, 2026
            // Last3Months current  = April 1  → June 30  (91 days: Apr30+May31+Jun30)
            // Last3Months previous = January 1 → March 31 (90 days: Jan31+Feb28+Mar31)

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, _userId, _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                // Current period — last 3 months
                (1000, ReferenceDate.AddMonths(-1).AddDays(-1)),  // June 14  
                (500,  ReferenceDate.AddMonths(-2).AddDays(-1)),  // May 14   
                (500,  ReferenceDate.AddMonths(-2).AddDays(-3)),  // May 12  
                (1500, ReferenceDate.AddMonths(-3).AddDays(-1)),  // April 14 

                // Previous period — 3-6 months ago
                (800,  ReferenceDate.AddMonths(-4).AddDays(-1)),  // March 14 
                (700,  ReferenceDate.AddMonths(-5).AddDays(-1)),  // Feb 14   
                (500,  ReferenceDate.AddMonths(-6).AddDays(-1)),  // Jan 14   
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, _userId, _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                // Current period
                (100,  ReferenceDate.AddMonths(-1).AddDays(-2)),  // June 13  
                (100,  ReferenceDate.AddMonths(-1).AddDays(-3)),  // June 12  
                (200,  ReferenceDate.AddMonths(-2).AddDays(-2)),  // May 13   
                (50,   ReferenceDate.AddMonths(-2).AddDays(-1)),  // May 14  
                (20,   ReferenceDate.AddMonths(-2).AddDays(-3)),  // May 12   
                (300,  ReferenceDate.AddMonths(-3).AddDays(-2)),  // April 13 
                (150,  ReferenceDate.AddMonths(-3).AddDays(-1)),  // April 14 

                // Previous period
                (400,  ReferenceDate.AddMonths(-4).AddDays(-2)),  // March 13 
                (300,  ReferenceDate.AddMonths(-5).AddDays(-1)),  // Feb 14   
                (200,  ReferenceDate.AddMonths(-6).AddDays(-3)),  // Jan 12   
                }, ct);

            // Current (Apr-Jun):  Income 3500, Expense 920,  Net 2580
            // Previous (Jan-Mar): Income 2000, Expense 900,  Net 1100
            // All-time balance = (3500+2000) - (920+900) = 3680

            // BalanceChange    = (2580 - 1100) / 1100 * 100 = 100 (clamped)
            // IncomeChange     = (3500 - 2000) / 2000 * 100 = 75%
            // ExpenseChange    = (920 - 900)   / 900  * 100 = 2.2%
            // AvgDaily current = 920 / 91  = 10.11
            // AvgDaily prev    = 900 / 90  = 10.00
            // AvgDailyChange   = (10.11 - 10.00) / 10.00 * 100 = 1.1%

            var result = await _handler.Handle(
                new GetDashboardSummaryQuery(_userId, TimePeriod.Last3Months), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalBalance.Amount.Should().Be(3680);
            result.Value.TotalBalance.PercentageChange.Should().Be(100);  // clamped
            result.Value.Income.Amount.Should().Be(3500);
            result.Value.Income.PercentageChange.Should().Be(75);
            result.Value.Expense.Amount.Should().Be(920);
            result.Value.Expense.PercentageChange.Should().Be(2.2m);
            result.Value.AverageDailySpending.Amount.Should().Be(10.11m);
            result.Value.AverageDailySpending.PercentageChange.Should().Be(1.1m);
        }

        [Fact]
        public async Task Handle_TimePeriodLast6Months_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, _userId, _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (500,  ReferenceDate.AddMonths(-6).AddDays(-1)), // Jan 14
                    (200, ReferenceDate.AddMonths(-6).AddDays(-2)), // Jan 15
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, _userId, _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (200,  ReferenceDate.AddMonths(-6).AddDays(-3)),  // Jan 13
                }, ct);

            var result = await _handler.Handle(
                new GetDashboardSummaryQuery(_userId, TimePeriod.Last6Months), ct);

            result.IsSuccess.Should().BeTrue();

            // Total Balance
            result.Value.TotalBalance.Amount.Should().Be(500);
            result.Value.TotalBalance.PercentageChange.Should().Be(100);

            // Income
            result.Value.Income.Amount.Should().Be(700);
            result.Value.Income.PercentageChange.Should().Be(100);

            // Expense
            result.Value.Expense.Amount.Should().Be(200);
            result.Value.Expense.PercentageChange.Should().Be(100);

            // Average Daily Spending
            result.Value.AverageDailySpending.Amount.Should().Be(1.10m);
            result.Value.AverageDailySpending.PercentageChange.Should().Be(100);
        }

        [Fact]
        public async Task Handle_TimePeriodThisYear_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, _userId, _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (500,  ReferenceDate.AddMonths(-6).AddDays(-1)), // Jan 14
                    (500, ReferenceDate.AddMonths(-5).AddDays(-10)), // Feb 15
                    (500, ReferenceDate.AddMonths(-4).AddDays(-20)),
                    (500, ReferenceDate.AddMonths(-3).AddDays(-12)),
                    (500, ReferenceDate.AddMonths(-2).AddDays(-8)),
                    (500, ReferenceDate.AddMonths(-1).AddDays(-25)),
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, _userId, _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (200,  ReferenceDate.AddMonths(-6).AddDays(-3)),  // Jan 13
                    (500,  ReferenceDate.AddMonths(-5).AddDays(-16)),
                    (50,  ReferenceDate.AddMonths(-4).AddDays(-9)),
                    (10,  ReferenceDate.AddMonths(-3).AddDays(-1)),
                    (120,  ReferenceDate.AddMonths(-2).AddDays(-6)),
                    (20,  ReferenceDate.AddMonths(-1).AddDays(-29)),
                    (10, ReferenceDate)
                }, ct);

            var result = await _handler.Handle(
                new GetDashboardSummaryQuery(_userId, TimePeriod.ThisYear), ct);

            result.Value.TotalBalance.Amount.Should().Be(2090);
            result.Value.TotalBalance.PercentageChange.Should().Be(100);

            result.Value.Income.Amount.Should().Be(3000);
            result.Value.Income.PercentageChange.Should().Be(100);

            result.Value.Expense.Amount.Should().Be(910);
            result.Value.Expense.PercentageChange.Should().Be(100);
        }

        [Fact]
        public async Task Handle_TimePeriodAllTime_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, _userId, _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (500,  ReferenceDate.AddMonths(-6).AddDays(-1)), 
                    (500, ReferenceDate.AddMonths(-5).AddDays(-10)), 
                    (500, ReferenceDate.AddMonths(-4).AddDays(-20)),
                    (500, ReferenceDate.AddMonths(-3).AddDays(-12)),
                    (500, ReferenceDate.AddMonths(-2).AddDays(-8)),
                    (500, ReferenceDate.AddMonths(-1).AddDays(-25)),
                    (2000, ReferenceDate.AddYears(-1))
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, _userId, _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (200,  ReferenceDate.AddMonths(-6).AddDays(-3)),  
                    (500,  ReferenceDate.AddMonths(-5).AddDays(-16)),
                    (50,  ReferenceDate.AddMonths(-4).AddDays(-9)),
                    (10,  ReferenceDate.AddMonths(-3).AddDays(-1)),
                    (120,  ReferenceDate.AddMonths(-2).AddDays(-6)),
                    (20,  ReferenceDate.AddMonths(-1).AddDays(-29)),
                    (10, ReferenceDate),
                    (1200, ReferenceDate.AddYears(-1).AddDays(-1))
                }, ct);

            var result = await _handler.Handle(
                new GetDashboardSummaryQuery(_userId, TimePeriod.AllTime), ct);

            result.Value.TotalBalance.Amount.Should().Be(2890);
            result.Value.TotalBalance.PercentageChange.Should().Be(100);

            result.Value.Income.Amount.Should().Be(5000);
            result.Value.Income.PercentageChange.Should().Be(100);

            result.Value.Expense.Amount.Should().Be(2110);
            result.Value.Expense.PercentageChange.Should().Be(100);
        }
    }
}
