using Auren.Application.Features.Dashboard.Queries.GetExpenseBreakdown;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using Auren.Tests.Features.Dashboard.Helpers;
using FluentAssertions;
using System.Linq;

namespace Auren.Tests.Features.Dashboard.Queries
{
    public class GetExpenseBreakdownHandlerTests
    {
        private static readonly DateTime ReferenceDate = new(2026, 7, 15);
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetExpenseBreakdownHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();
        private Category _foodCategory = null!;
        private Category _incomeCategory = null!;
        private Category _transportCategory = null!;

        public GetExpenseBreakdownHandlerTests()
        {
            var dateTime = new FakeDateTimeProvider(ReferenceDate);
            _handler = new GetExpenseBreakdownHandler(_db, dateTime);
        }

        private async Task SetupCategoriesAsync(CancellationToken ct)
        {
            _foodCategory = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.Name, "Food")
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            _transportCategory = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.Name, "Transport")
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            _incomeCategory = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.Name, "Salary")
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate();

            await _db.Categories.AddRangeAsync(
                [_foodCategory, _transportCategory, _incomeCategory], ct);
            await _db.SaveChangesAsync(ct);
        }

        [Fact]
        public async Task Handle_NoTransactions_ReturnsEmptyBreakdown()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Labels.Should().BeEmpty();
            result.Value.Amounts.Should().BeEmpty();
            result.Value.Percentages.Should().BeEmpty();
            result.Value.BackgroundColors.Should().BeEmpty();
            result.Value.TotalSpent.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithExpenses_ReturnsCorrectLabelsAndAmounts()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await _db.Transactions.AddRangeAsync(
                [
                    Fakers.TransactionFaker(_userId)
                        .RuleFor(t => t.CategoryId, _foodCategory.Id)
                        .RuleFor(t => t.TransactionType, TransactionType.Expense)
                        .RuleFor(t => t.Amount, 300)
                        .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                        .Generate(),

                    Fakers.TransactionFaker(_userId)
                        .RuleFor(t => t.CategoryId, _transportCategory.Id)
                        .RuleFor(t => t.TransactionType, TransactionType.Expense)
                        .RuleFor(t => t.Amount, 100)
                        .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-2))
                        .Generate()
                ], ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Labels.Should().HaveCount(2);
            result.Value.Labels.ElementAt(0).Should().Be("Food");
            result.Value.Labels.ElementAt(1).Should().Be("Transport");
            result.Value.Amounts.ElementAt(0).Should().Be(300m);
            result.Value.Amounts.ElementAt(1).Should().Be(100m);
            result.Value.TotalSpent.Should().Be(400);
        }

        [Fact]
        public async Task Handle_WithExpenses_ReturnsCorrectPercentages()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await _db.Transactions.AddRangeAsync(
            [
                Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _foodCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 300)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                .Generate(),

            Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _transportCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 100)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-2))
                .Generate()
            ], ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.Value.Percentages.ElementAt(0).Should().Be(75);
            result.Value.Percentages.ElementAt(1).Should().Be(25);
            result.Value.Percentages.Sum().Should().Be(100);
        }

        [Fact]
        public async Task Handle_WithExpenses_ReturnsOrderedByAmountDescending()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await _db.Transactions.AddRangeAsync(
            [
                Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _transportCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 100)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                .Generate(),

            Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _foodCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 300)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-2))
                .Generate()
            ], ct);

            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.Value.Labels.ElementAt(0).Should().Be("Food");
            result.Value.Labels.ElementAt(1).Should().Be("Transport");
            result.Value.Amounts.ElementAt(0).Should().BeGreaterThan(result.Value.Amounts.ElementAt(1));
        }

        [Fact]
        public async Task Handle_WithIncomeAndExpense_ExcludesIncome()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await _db.Transactions.AddRangeAsync(
            [
                Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _incomeCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Income)
                .RuleFor(t => t.Amount, 5000)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                .Generate(),

                Fakers.TransactionFaker(_userId)
                    .RuleFor(t => t.CategoryId, _foodCategory.Id)
                    .RuleFor(t => t.TransactionType, TransactionType.Expense)
                    .RuleFor(t => t.Amount, 300)
                    .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                    .Generate()
             ], ct);

            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Labels.Should().ContainSingle();
            result.Value.Labels.Should().Contain("Food");
            result.Value.Labels.Should().NotContain("Salary");  
            result.Value.TotalSpent.Should().Be(300);            
        }

        [Fact]
        public async Task Handle_WithExpenses_ReturnsBackgroundColorsForEachCategory()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await _db.Transactions.AddRangeAsync(
            [
                Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _foodCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 300)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                .Generate(),

            Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _transportCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 100)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-2))
                .Generate()
            ], ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.Value.BackgroundColors.Should().HaveCount(2);
            result.Value.BackgroundColors.Should().AllSatisfy(
                c => c.Should().StartWith("rgba("));  

            result.Value.BackgroundColors.ElementAt(0).Should().NotBe(result.Value.BackgroundColors.ElementAt(1));
        }

        [Fact]
        public async Task Handle_OtherUserExpenses_AreNotReturned()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            var otherUserId = Guid.NewGuid();
            var otherCategory = Fakers.CategoryFaker(otherUserId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(otherCategory, ct);

            await _db.Transactions.AddAsync(
                Fakers.TransactionFaker(otherUserId)
                    .RuleFor(t => t.CategoryId, otherCategory.Id)
                    .RuleFor(t => t.TransactionType, TransactionType.Expense)
                    .RuleFor(t => t.Amount, 500)
                    .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                    .Generate(), ct);

            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Labels.Should().BeEmpty();   
            result.Value.TotalSpent.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ExpensesOutsidePeriod_AreNotReturned()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await _db.Transactions.AddAsync(
                Fakers.TransactionFaker(_userId)
                    .RuleFor(t => t.CategoryId, _foodCategory.Id)
                    .RuleFor(t => t.TransactionType, TransactionType.Expense)
                    .RuleFor(t => t.Amount, 300)
                    .RuleFor(t => t.TransactionDate, ReferenceDate.AddMonths(-2)) 
                    .Generate(), ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Labels.Should().BeEmpty();  
            result.Value.TotalSpent.Should().Be(0);
        }

        [Fact]
        public async Task Handle_MultipleTransactionsSameCategory_AggregatesToOneLabel()
        {
            var ct = TestContext.Current.CancellationToken;
            await SetupCategoriesAsync(ct);

            await _db.Transactions.AddRangeAsync(
            [
                Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _foodCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 100)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-1))
                .Generate(),

            Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _foodCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 200)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-2))
                .Generate(),

            Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, _foodCategory.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.Amount, 50)
                .RuleFor(t => t.TransactionDate, ReferenceDate.AddDays(-3))
                .Generate()
            ], ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetExpenseBreakdownQuery(_userId, TimePeriod.ThisMonth), ct);

            result.Value.Labels.Should().ContainSingle();
            result.Value.Labels.ElementAt(0).Should().Be("Food");
            result.Value.Amounts.ElementAt(0).Should().Be(350);    
            result.Value.Percentages.ElementAt(0).Should().Be(100); 
            result.Value.TotalSpent.Should().Be(350);
        }
    }
}
