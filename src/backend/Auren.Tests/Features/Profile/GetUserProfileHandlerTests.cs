using Auren.Application.Common.Result;
using Auren.Application.Features.Profile.Queries.GetUserProfile;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Auren.Tests.Features.Profile
{
    public class GetUserProfileHandlerTests
    {
        private readonly AurenAuthDbContext _db = TestDbContextFactory.CreateAuthDb();
        private readonly GetUserProfileHandler _handler;

        public GetUserProfileHandlerTests()
        {
            _handler = new GetUserProfileHandler(_db);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var user = Fakers.UserFaker().Generate();

            await _db.Users.AddAsync(user, ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(new GetUserProfileQuery(user.Id), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Email.Should().Be(user.Email);
            result.Value.FirstName.Should().Be(user.FirstName);
            result.Value.LastName.Should().Be(user.LastName);
            result.Value.ProfilePictureUrl.Should().Be(user.ProfilePictureUrl);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var result = await _handler.Handle(new GetUserProfileQuery(Guid.NewGuid()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
            result.Error.Messages.Should().Contain("User not found.");
        }
    }
}