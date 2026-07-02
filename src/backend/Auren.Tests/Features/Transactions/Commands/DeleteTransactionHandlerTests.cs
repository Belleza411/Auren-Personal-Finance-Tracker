using Auren.Application.Common.Result;
using Auren.Application.Features.Transactions.Commands.DeleteTransaction;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Auren.Tests.Features.Transactions.Commands
{
    public class DeleteTransactionHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly DeleteTransactionHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public DeleteTransactionHandlerTests()
        {
            _handler = new DeleteTransactionHandler(_db);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var transaction = Fakers.TransactionFaker(_userId).Generate();

            await _db.Transactions.AddAsync(transaction, ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new DeleteTransactionCommand(_userId, transaction.Id), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();

            var deletedTransaction = await _db.Transactions
                .FirstOrDefaultAsync(t => t.Id == transaction.Id, ct);

            deletedTransaction.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var result = await _handler.Handle(
                new DeleteTransactionCommand(_userId, Guid.NewGuid()), ct);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
        }
    }
}
