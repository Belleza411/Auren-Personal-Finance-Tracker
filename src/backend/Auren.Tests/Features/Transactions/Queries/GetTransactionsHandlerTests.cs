using Auren.Application.Features.Transactions.DTOs;
using Auren.Application.Features.Transactions.Queries.GetTransactions;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;

namespace Auren.Tests.Features.Transactions.Queries
{
    public class GetTransactionsHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetTransactionsHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public GetTransactionsHandlerTests()
        {
            _handler = new GetTransactionsHandler(_db);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsPagedResult()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            var transaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddAsync(transaction, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter
                {
                    Category = [category.Name],
                },
                PageNumber: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle();
            result.Value.Items.First().Id.Should().Be(transaction.Id);
        }

        [Fact]
        public async Task Handle_NoTransactions_ReturnsEmptyPagedResult()
        {
            var ct = TestContext.Current.CancellationToken;

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter(),
                PageNumber: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_SearchTerm_ReturnsPagedResultWithMatchingTransactions()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var transaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.Name, "Grocery Shopping")
                .Generate();

            var nonMatching = Fakers.TransactionFaker(_userId)
               .RuleFor(t => t.CategoryId, category.Id)
               .RuleFor(t => t.Name, "Netflix Subscription")
               .Generate();

            await _db.Transactions.AddAsync(transaction, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter
                {
                    SearchTerm = "Grocery"
                },
                PageNumber: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle();
            result.Value.Items.Should().Contain(x => x.Name.Contains("Grocery"));
            result.Value.Items.First().Id.Should().Be(transaction.Id);
        }

        [Fact]
        public async Task Handle_DateRange_ReturnPagedResult()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            var inRange = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionDate, new DateTime(2026, 7, 3))
                .Generate();

            var outOfRange = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionDate, new DateTime(2026, 8, 1))
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddRangeAsync([inRange, outOfRange], ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter
                {
                    StartDate = new DateTime(2026, 7, 1),
                    EndDate = new DateTime(2026, 7, 5)
                },
                PageNumber: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.First().Id.Should().Be(inRange.Id);
            result.Value.TotalCount.Should().Be(1);
            result.Value.Items.Should().NotContain(x => x.Id == outOfRange.Id);
        }


        [Fact]
        public async Task Handle_TransactionTypeFilter_ReturnsPagedResultWithMatchingTransactions()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var expenseTransaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .Generate();

            var incomeTransaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Income)
                .Generate();

            await _db.Transactions.AddRangeAsync([expenseTransaction, incomeTransaction], ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter
                {
                    TransactionType = TransactionType.Expense
                },
                PageNumber: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle();
            result.Value.Items.First().Id.Should().Be(expenseTransaction.Id);
            result.Value.TotalCount.Should().Be(1);
            result.Value.Items.Should().NotContain(x => x.Id == incomeTransaction.Id);
        }

        [Fact]
        public async Task Handle_PaymentTypeFilter_ReturnsPagedResultWithMatchingTransactions()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            var creditCard = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Expense)
                .RuleFor(t => t.PaymentType, PaymentType.CreditCard)
                .Generate();

            var cash = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, TransactionType.Income)
                .RuleFor(t => t.PaymentType, PaymentType.Cash)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddRangeAsync([creditCard, cash], ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter
                {
                    PaymentType = PaymentType.CreditCard
                },
                PageNumber: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().ContainSingle();
            result.Value.Items.First().Id.Should().Be(creditCard.Id);
            result.Value.TotalCount.Should().Be(1);
            result.Value.Items.Should().NotContain(x => x.Id == cash.Id);
        }

        [Fact]
        public async Task Handle_PaginationPageNumber_ReturnsPagedResultWithCorrectPageNumber()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId).Generate();
            var transactions = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .Generate(15);

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddRangeAsync(transactions, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter(),
                PageNumber: 2,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(5);
            result.Value.TotalCount.Should().Be(15);
            result.Value.PageNumber.Should().Be(2);
        }

        [Fact]
        public async Task Handle_PaginationPageNumber_ReturnsPagedResultWithCorrectPageSize()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId).Generate();
            var transactions = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .Generate(25);

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddRangeAsync(transactions, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter(),
                PageNumber: 1,
                PageSize: 20
            );

            var result = await _handler.Handle(query, ct);
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().HaveCount(20);
            result.Value.TotalCount.Should().Be(25);
            result.Value.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task Handle_OtherUserTransactions_ReturnsPagedResult()
        {
            var ct = TestContext.Current.CancellationToken;

            var otherUserId = Guid.NewGuid();

            var category = Fakers.CategoryFaker(otherUserId).Generate();
            var otherUserTransactions = Fakers.TransactionFaker(otherUserId)
                .RuleFor(t => t.CategoryId, category.Id)
                .Generate(25);

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddRangeAsync(otherUserTransactions, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetTransactionsQuery(
                UserId: _userId,
                Filter: new TransactionFilter(),
                PageNumber: 1,
                PageSize: 10
            );

            var result = await _handler.Handle(query, ct);
            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
            result.Value.TotalCount.Should().Be(0);
        }
    }
}
