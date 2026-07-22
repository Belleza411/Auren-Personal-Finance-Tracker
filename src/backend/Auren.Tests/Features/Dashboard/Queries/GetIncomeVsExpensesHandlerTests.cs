using Auren.Application.Features.Dashboard.Queries.GetIncomesVsExpenses;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using Auren.Tests.Features.Dashboard.Helpers;
using FluentAssertions;
using System.Linq;

namespace Auren.Tests.Features.Dashboard.Queries
{
    public class GetIncomesVsExpensesHandlerTests
    {
        private static readonly DateTime ReferenceDate = new(2026, 7, 15);
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetIncomesVsExpensesHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();
        private Category _incomeCategory = null!;
        private Category _expenseCategory = null!;

        public GetIncomesVsExpensesHandlerTests()
        {
            var dateTime = new FakeDateTimeProvider(ReferenceDate);
            _handler = new GetIncomesVsExpensesHandler(_db, dateTime);
        }

        private async Task SetupCategoriesAsync(CancellationToken ct)
        {
            (_incomeCategory, _expenseCategory) =
                await DashboardSeedHelper.SeedCategoriesAsync(_db, _userId, ct);
        }


        [Fact]
        public async Task Handle_NoTransactions_ReturnsEmptyResponse()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Labels.Should().NotBeEmpty();  
            result.Value.Incomes.Should().AllSatisfy(i => i.Should().Be(0));
            result.Value.Expenses.Should().AllSatisfy(e => e.Should().Be(0));
        }

        [Fact]
        public async Task Handle_ThisMonth_ReturnsDailyData()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(_db, _userId, _incomeCategory.Id,
            [
                (1000, ReferenceDate.AddDays(-1)),   
                (500,  ReferenceDate.AddDays(-5)),   
            ], ct);

            await DashboardSeedHelper.SeedExpensesAsync(_db, _userId, _expenseCategory.Id,
            [
                (200, ReferenceDate.AddDays(-2)),    
                (100, ReferenceDate.AddDays(-5)),    
            ], ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.Labels.Should().AllSatisfy(
                l => l.Should().MatchRegex(@"^[A-Za-z]+ \d+$")); 

            result.Value.Labels.Should().Contain("Jul 1");
            result.Value.Labels.Should().Contain("Jul 31");

            result.Value.Labels.Should().Contain("Jul 14");
            result.Value.Labels.Should().Contain("Jul 10");
            result.Value.Labels.Should().Contain("Jul 13");

            result.Value.Incomes.Sum().Should().Be(1500);  
            result.Value.Expenses.Sum().Should().Be(300);  
        }

        [Fact]
        public async Task Handle_LastMonth_ReturnsDailyData()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(_db, _userId, _incomeCategory.Id,
            [
                (1000, ReferenceDate.AddMonths(-1).AddDays(-1)),  
            ], ct);

            await DashboardSeedHelper.SeedExpensesAsync(_db, _userId, _expenseCategory.Id,
            [
                (200, ReferenceDate.AddMonths(-1).AddDays(-2)),   
            ], ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.LastMonth), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.Labels.Should().AllSatisfy(
                l => l.Should().MatchRegex(@"^[A-Za-z]+ \d+$"));

            result.Value.Labels.Should().Contain("Jun 1");
            result.Value.Labels.Should().Contain("Jun 30");

            result.Value.Incomes.Sum().Should().Be(1000);
            result.Value.Expenses.Sum().Should().Be(200);
        }

        [Fact]
        public async Task Handle_Last3Months_ReturnsMonthlyData()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(_db, _userId, _incomeCategory.Id,
            [
                (1000, ReferenceDate.AddMonths(-1).AddDays(-1)), 
                (500,  ReferenceDate.AddMonths(-2).AddDays(-1)),  
                (800,  ReferenceDate.AddMonths(-3).AddDays(-1)),  
            ], ct);

            await DashboardSeedHelper.SeedExpensesAsync(_db, _userId, _expenseCategory.Id,
            [
                (200, ReferenceDate.AddMonths(-1).AddDays(-2)),   
                (150, ReferenceDate.AddMonths(-2).AddDays(-2)),   
                (100, ReferenceDate.AddMonths(-3).AddDays(-2)),   
            ], ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.Last3Months), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.Labels.Should().AllSatisfy(
                l => l.Should().MatchRegex(@"^[A-Za-z]+ \d{4}$")); 

            result.Value.Labels.Should().Contain("Apr 2026");
            result.Value.Labels.Should().Contain("May 2026");
            result.Value.Labels.Should().Contain("Jun 2026");
            result.Value.Labels.Should().HaveCount(3);

            result.Value.Labels.ElementAt(0).Should().Be("Apr 2026");
            result.Value.Labels.ElementAt(1).Should().Be("May 2026");
            result.Value.Labels.ElementAt(2).Should().Be("Jun 2026");

            result.Value.Incomes.ElementAt(0).Should().Be(800);   
            result.Value.Incomes.ElementAt(1).Should().Be(500);   
            result.Value.Incomes.ElementAt(2).Should().Be(1000);  

            result.Value.Expenses.ElementAt(0).Should().Be(100);  
            result.Value.Expenses.ElementAt(1).Should().Be(150);  
            result.Value.Expenses.ElementAt(2).Should().Be(200);  
        }

        [Fact]
        public async Task Handle_ThisYear_ReturnsMonthlyData()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);
            await DashboardSeedHelper.SeedIncomesAsync(_db, _userId, _incomeCategory.Id,
            [
                (1000, new DateTime(2026, 1, 15)),   
                (1500, new DateTime(2026, 3, 10)),   
                (2000, new DateTime(2026, 6, 20)),   
            ], ct);

            await DashboardSeedHelper.SeedExpensesAsync(_db, _userId, _expenseCategory.Id,
            [
                (300, new DateTime(2026, 1, 20)),    
                (500, new DateTime(2026, 3, 15)),   
                (700, new DateTime(2026, 6, 25)),    
            ], ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.ThisYear), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.Labels.Should().Contain("Jan 2026");
            result.Value.Labels.Should().Contain("Mar 2026");
            result.Value.Labels.Should().Contain("Jun 2026");
            result.Value.Labels.Should().HaveCount(3); 


            result.Value.Labels.ElementAt(0).Should().Be("Jan 2026");
            result.Value.Labels.ElementAt(1).Should().Be("Mar 2026");
            result.Value.Labels.ElementAt(2).Should().Be("Jun 2026");

            result.Value.Incomes.Sum().Should().Be(4500);   
            result.Value.Expenses.Sum().Should().Be(1500);  
        }

        [Fact]
        public async Task Handle_LabelsIncomesExpenses_AlwaysHaveSameCount()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(_db, _userId, _incomeCategory.Id,
            [
                (1000, ReferenceDate.AddDays(-1)),
                (500,  ReferenceDate.AddDays(-5)),
                (300,  ReferenceDate.AddDays(-10)),
            ], ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();

            result.Value.Labels.Count().Should().Be(result.Value.Incomes.Count());
            result.Value.Labels.Count().Should().Be(result.Value.Expenses.Count());
        }

        [Fact]
        public async Task Handle_OtherUserTransactions_AreNotReturned()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            var otherUserId = Guid.NewGuid();
            var otherCategory = Fakers.CategoryFaker(otherUserId)
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate();

            await _db.Categories.AddAsync(otherCategory, ct);
            await _db.Transactions.AddAsync(
                Fakers.TransactionFaker(otherUserId)
                    .RuleFor(t => t.CategoryId, otherCategory.Id)
                    .RuleFor(t => t.TransactionType, TransactionType.Income)
                    .RuleFor(t => t.Amount, 5000)
                    .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                    .Generate(), ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Incomes.Sum().Should().Be(0);   
            result.Value.Expenses.Sum().Should().Be(0);
        }

        [Fact]
        public async Task Handle_TransactionsOutsidePeriod_AreNotReturned()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await DashboardSeedHelper.SeedIncomesAsync(_db, _userId, _incomeCategory.Id,
            [
                (1000, ReferenceDate.AddMonths(-2)),  
            ], ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Incomes.Sum().Should().Be(0);  
        }

        [Fact]
        public async Task Handle_MultipleSameDayTransactions_AggregatesToOneEntry()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            var sameDay = ReferenceDate.AddDays(-1); 

            await DashboardSeedHelper.SeedIncomesAsync(_db, _userId, _incomeCategory.Id,
            [
                (500, sameDay),
                (300, sameDay),
                (200, sameDay),
            ], ct);

            var result = await _handler.Handle(
                new GetIncomesVsExpensesQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();

            var labels = result.Value.Labels.ToList();
            var jul14Index = labels.IndexOf("Jul 14");
            jul14Index.Should().BeGreaterThanOrEqualTo(0);
            result.Value.Incomes.ElementAt(jul14Index).Should().Be(1000);
        }
    }
}
