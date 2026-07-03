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
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetDashboardSummaryHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();
        private Category _incomeCategory = null!;
        private Category _expenseCategory = null!;

        public GetDashboardSummariesHandlerTests()
        {
            _handler = new GetDashboardSummaryHandler(_db);
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
    }
}
