using Auren.Application.Common.Result;
using Auren.Application.Features.Transactions.Queries.GetTransactionById;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;

namespace Auren.Tests.Features.Transactions.Queries
{
    public class GetTransactionByIdHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetTransactionByIdHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public GetTransactionByIdHandlerTests()
        {
            _handler = new GetTransactionByIdHandler(_db);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsSuccess()
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

            var result = await _handler.Handle(
                new GetTransactionByIdQuery(transaction.Id, _userId), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(transaction.Id);
            result.Value.UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var result = await _handler.Handle(
                new GetTransactionByIdQuery(Guid.NewGuid(), _userId), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
        }

        [Fact]
        public async Task Handle_OtherUserTransaction_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var otherUserId = Guid.NewGuid();

            var category = Fakers.CategoryFaker(otherUserId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            var otherUserTransaction = Fakers.TransactionFaker(otherUserId)
                .RuleFor(t => t.CategoryId, category.Id)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddAsync(otherUserTransaction, ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetTransactionByIdQuery(otherUserTransaction.Id, _userId), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
            result.Error.Messages.Should().Contain("Transaction not found");
        }
    }
}
