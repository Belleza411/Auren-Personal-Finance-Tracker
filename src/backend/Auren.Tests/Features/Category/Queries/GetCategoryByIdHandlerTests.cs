using Auren.Application.Common.Result;
using Auren.Application.Features.Categories.Queries.GetCategoryById;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Tests.Features.Category.Queries
{
    public class GetCategoryByIdHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetCategoryByIdHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public GetCategoryByIdHandlerTests()
        {
            _handler = new GetCategoryByIdHandler(_db);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId).Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var result = await _handler.Handle(
                new GetCategoryByIdCommand(_userId, category.Id), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(category.Id);
            result.Value.UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var result = await _handler.Handle(
                new GetCategoryByIdCommand(_userId, Guid.NewGuid()), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
        }

        [Fact]
        public async Task Handle_OtherUserCategory_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var otherUserId = Guid.NewGuid();

            var category = Fakers.CategoryFaker(otherUserId).Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var isExists = await _handler.Handle(
                new GetCategoryByIdCommand(_userId, category.Id), ct);

            isExists.IsFailure.Should().BeTrue();
            isExists.Error.Code.Should().Be(ErrorTypes.NotFound);

            var otherUserCategory = await _db.Categories
                .FirstOrDefaultAsync(c => c.UserId == otherUserId, ct);

            otherUserCategory.Should().NotBeNull();
            otherUserCategory.UserId.Should().Be(otherUserId);
        }
    }
}
