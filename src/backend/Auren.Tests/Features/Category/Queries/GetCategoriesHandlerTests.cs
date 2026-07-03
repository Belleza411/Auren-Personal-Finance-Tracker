using Auren.Application.Features.Categories.DTOs;
using Auren.Application.Features.Categories.Queries.GetCategories;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Auren.Tests.Features.Category.Queries
{
    public class GetCategoriesHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly GetCategoriesHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public GetCategoriesHandlerTests()
        {
            _handler = new GetCategoriesHandler(_db);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedResult()
        {
            var ct = TestContext.Current.CancellationToken;

            var categories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(10);

            await _db.Categories.AddRangeAsync(categories, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter(),
                PageSize: 10,
                PageNumber: 1
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalCount.Should().Be(10);
            result.Value.Items.Should().HaveCount(10);
            result.Value.PageNumber.Should().Be(1);
            result.Value.PageSize.Should().Be(10);
            result.Value.Items.First().UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_SearchTerm_ReturnPagedResultWithMatchingCategories()
        {
            var ct = TestContext.Current.CancellationToken;

            var categories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(9);
            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .RuleFor(c => c.Name, "PakNSave")
                .Generate();

            await _db.Categories.AddRangeAsync([..categories, category], ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter
                {
                    SearchTerm = "PakNSave"
                },
                PageSize: 10,
                PageNumber: 1
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalCount.Should().Be(1);
            result.Value.Items.Should().HaveCount(1);
            result.Value.Items.First().Name.Should().Be("PakNSave");
            result.Value.Items.First().UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_TransactionTypeExpense_ReturnPagedResultWithMatchingCategories()
        {
            var ct = TestContext.Current.CancellationToken;

            var expenseCategories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(10);

            var incomeCategories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate(10);

            await _db.Categories.AddRangeAsync([.. expenseCategories, .. incomeCategories], ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter
                {
                    TransactionType = TransactionType.Expense
                },
                PageSize: 10,
                PageNumber: 1
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalCount.Should().Be(10);
            result.Value.Items.Should().HaveCount(10);
            result.Value.Items.First().TransactionType.Should().Be(TransactionType.Expense);
            result.Value.Items.First().UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_TransactionTypeIncome_ReturnPagedResultWithMatchingCategories()
        {
            var ct = TestContext.Current.CancellationToken;

            var expenseCategories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(10);

            var incomeCategories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate(10);

            await _db.Categories.AddRangeAsync([.. expenseCategories, .. incomeCategories], ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter
                {
                    TransactionType = TransactionType.Income
                },
                PageSize: 10,
                PageNumber: 1
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalCount.Should().Be(10);
            result.Value.Items.Should().HaveCount(10);
            result.Value.Items.First().TransactionType.Should().Be(TransactionType.Income);
            result.Value.Items.First().UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_CategoriesFilter_ReturnsPagedResultWithMatchingCategories()
        {
            var ct = TestContext.Current.CancellationToken;

            var categories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(9);
            var grocery = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .RuleFor(c => c.Name, "Grocery")
                .Generate();

            await _db.Categories.AddRangeAsync([.. categories, grocery], ct);
            await _db.SaveChangesAsync(ct);

            var categoryNames = categories.Select(c => c.Name).Take(5).ToList();

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter
                {
                    Categories = [..categoryNames, grocery.Name]
                },
                PageSize: 10,
                PageNumber: 1
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.TotalCount.Should().Be(10);
            result.Value.Items.Should().HaveCount(10);
            result.Value.Items.Any(c => categoryNames.Contains(c.Name)).Should().BeTrue();
            result.Value.Items.Any(c => c.Name.Contains("Grocery")).Should().BeTrue();
            result.Value.Items.First().UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_PaginationPageNumber_ReturnsPagedResultWithCorrectPageNumber()
        {
            var ct = TestContext.Current.CancellationToken;

            var categories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(15);

            await _db.Categories.AddRangeAsync(categories, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter(),
                PageSize: 10,
                PageNumber: 2
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.PageSize.Should().Be(10);
            result.Value.PageNumber.Should().Be(2);
            result.Value.TotalCount.Should().Be(15);
            result.Value.Items.Should().HaveCount(5);
            result.Value.Items.First().UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_PaginationPageSize_ReturnPagedResultWithCorrectPageSize()
        {
            var ct = TestContext.Current.CancellationToken;

            var categories = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(25);

            await _db.Categories.AddRangeAsync(categories, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter(),
                PageSize: 20,
                PageNumber: 1
            );

            var result = await _handler.Handle(query, ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.PageSize.Should().Be(20);
            result.Value.PageNumber.Should().Be(1);
            result.Value.TotalCount.Should().Be(25);
            result.Value.Items.Should().HaveCount(20);
            result.Value.Items.First().UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_OtherUserCategories_ReturnsPagedResultWithNoCategories()
        {
            var ct = TestContext.Current.CancellationToken;

            var otherUser = Guid.NewGuid();

            var categories = Fakers.CategoryFaker(otherUser)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate(25);

            await _db.Categories.AddRangeAsync(categories, ct);
            await _db.SaveChangesAsync(ct);

            var query = new GetCategoriesQuery
            (
                UserId: _userId,
                Filter: new CategoriesFilter(),
                PageSize: 20,
                PageNumber: 1
            );

            var isExists = await _handler.Handle(query, ct);

            isExists.IsSuccess.Should().BeTrue();
            isExists.Value.PageSize.Should().Be(20);
            isExists.Value.PageNumber.Should().Be(1);
            isExists.Value.TotalCount.Should().Be(0);
            isExists.Value.Items.Should().HaveCount(0);
            isExists.Value.Items.Should().BeEmpty();

            var otherUserCategories = await _db.Categories
                .Where(c => c.UserId == otherUser)
                .ToListAsync(ct);

            otherUserCategories.Should().HaveCount(25);
            otherUserCategories.All(c => c.UserId == otherUser).Should().BeTrue();
        }
    }
}