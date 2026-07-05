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
            await DashboardSeedHelper.SeedIncomesAsync(
                _db, 
                _userId, 
                _incomeCategory.Id, 
                new List<(decimal, DateTime)>
            {
                (200, DateTime.UtcNow.AddDays(-1)), 
                (100, DateTime.UtcNow.AddDays(-3)),
                (100, DateTime.UtcNow.AddDays(-2))
            }, ct); 

            await DashboardSeedHelper.SeedExpensesAsync(
                _db,
                _userId,
                _expenseCategory.Id,
                new List<(decimal, DateTime)>
            {
                (50, DateTime.UtcNow.AddDays(-2)),
                (75, DateTime.UtcNow.AddDays(-1)),
                (10, DateTime.UtcNow.AddDays(-3))
            }, ct); 

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
            result.Value.AverageDailySpending.Amount.Should().Be(9m);
            result.Value.AverageDailySpending.PercentageChange.Should().Be(100);
        }

        [Fact]
        public async Task Handle_TimePeriodThisMonth_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;

            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(
                _db,
                _userId,
                _incomeCategory.Id,
                new List<(decimal, DateTime)>
            {
                (1000, DateTime.UtcNow.AddDays(-1)),
            }, ct); 

            await DashboardSeedHelper.SeedExpensesAsync(
                _db,
                _userId,
                _expenseCategory.Id,
                new List<(decimal, DateTime)>
            {
                (200, DateTime.UtcNow.AddDays(-2)),
                (50, DateTime.UtcNow.AddDays(-1)),
                (20, DateTime.UtcNow.AddDays(-3))
            }, ct);

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
            result.Value.AverageDailySpending.Amount.Should().Be(18m);
            result.Value.AverageDailySpending.PercentageChange.Should().Be(100);
        }

        [Fact]
        public async Task Handle_TimePeriodLastMonth_ReturnsCorrectSummary()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(
                _db, 
                _userId, 
                _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (1000, DateTime.Today.AddMonths(-1).AddDays(-1)), 
                    (500, DateTime.Today.AddMonths(-2).AddDays(-1)),   
                    (500, DateTime.Today.AddMonths(-2).AddDays(-3))    
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db, 
                _userId, 
                _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (100, DateTime.Today.AddMonths(-1).AddDays(-2)),   
                    (100, DateTime.Today.AddMonths(-1).AddDays(-3)), 
                    (200, DateTime.Today.AddMonths(-2).AddDays(-2)),   
                    (50,  DateTime.Today.AddMonths(-2).AddDays(-1)),   
                    (20,  DateTime.Today.AddMonths(-2).AddDays(-3))    
                }, ct);

            var result = await _handler.Handle(
                new GetDashboardSummaryQuery(_userId, TimePeriod.LastMonth), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.TotalBalance.Amount.Should().Be(1530);
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

            await DashboardSeedHelper.SeedIncomesAsync(
                _db,
                _userId,
                _incomeCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (1000, DateTime.Today.AddMonths(-1).AddDays(-1)),
                    (500, DateTime.Today.AddMonths(-2).AddDays(-1)),
                    (500, DateTime.Today.AddMonths(-2).AddDays(-3)),
                    (1500, DateTime.Today.AddMonths(-3).AddDays(-1)),
                }, ct);

            await DashboardSeedHelper.SeedExpensesAsync(
                _db,
                _userId,
                _expenseCategory.Id,
                new List<(decimal, DateTime)>
                {
                    (100, DateTime.Today.AddMonths(-1).AddDays(-2)),
                    (100, DateTime.Today.AddMonths(-1).AddDays(-3)),
                    (200, DateTime.Today.AddMonths(-2).AddDays(-2)),
                    (50,  DateTime.Today.AddMonths(-2).AddDays(-1)),
                    (20,  DateTime.Today.AddMonths(-2).AddDays(-3)),
                    (300, DateTime.Today.AddMonths(-3).AddDays(-2)),
                    (150, DateTime.Today.AddMonths(-3).AddDays(-1)),
                }, ct);

            var result = await _handler.Handle(
                new GetDashboardSummaryQuery(_userId, TimePeriod.Last3Months), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.TotalBalance.Amount.Should().Be(2580);
            result.Value.TotalBalance.PercentageChange.Should().Be(43.83m);

            result.Value.Income.Amount.Should().Be(1500);
            result.Value.Income.PercentageChange.Should().Be(100);

            result.Value.Expense.Amount.Should().Be(150);
            result.Value.Expense.PercentageChange.Should().Be(-70m);

            result.Value.AverageDailySpending.Amount.Should().Be(4.84m);
            result.Value.AverageDailySpending.PercentageChange.Should().Be(-69.98m);
        }
    }
}
