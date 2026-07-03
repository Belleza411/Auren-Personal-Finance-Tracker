using Auren.Application.Common.Result;
using Auren.Application.Features.Categories.Commands.DeleteCategory;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Auren.Tests.Features.Category.Commands
{
    public class DeleteCategoryHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly DeleteCategoryHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public DeleteCategoryHandlerTests()
        {
            _handler = new DeleteCategoryHandler(_db);
        }

        [Fact]
        public async Task Handle_CanDelete_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId).Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new DeleteCategoryCommand(_userId, category.Id), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();

            var deletedCategory = await _db.Categories
                .FirstOrDefaultAsync(c => 
                    c.Id == category.Id &&
                    c.UserId == _userId, ct);

            deletedCategory.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var result = await _handler.Handle(
              new DeleteCategoryCommand(_userId, Guid.NewGuid()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
        }

        [Fact]
        public async Task Handle_DeleteOtherUserCategory_ReturnsNotFound()
        {
            var ct = TestContext.Current.CancellationToken;

            var otherUserId = Guid.NewGuid();

            var category = Fakers.CategoryFaker(otherUserId).Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new DeleteCategoryCommand(_userId, category.Id), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);

            var stillExists = await _db.Categories
                .FirstOrDefaultAsync(c => c.Id == category.Id, ct);

            stillExists.Should().NotBeNull();
        }
    }
}
