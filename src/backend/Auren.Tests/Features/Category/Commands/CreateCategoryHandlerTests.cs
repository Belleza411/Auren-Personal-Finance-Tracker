using Auren.Application.Common.Result;
using Auren.Application.Features.Categories.Commands.CreateCategory;
using Auren.Application.Features.Categories.DTOs;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Auren.Tests.Features.Category.Commands
{
    public class CreateCategoryHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly Mock<IValidator<CategoryDto>> _validator = new();
        private readonly CreateCategoryHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public CreateCategoryHandlerTests()
        {
            _handler = new CreateCategoryHandler(_db, _validator.Object);
        }

        [Fact]
        public async Task Handle_ValidCategory_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var dto = Fakers.CategoryDtoFaker()
                .RuleFor(c => c.Name, "Groceries")
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(
                new CreateCategoryCommand(dto, _userId), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(dto.Name);
            result.Value.UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_ValidationFailed_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var dto = Fakers.CategoryDtoFaker().Generate();

            var failures = new List<ValidationFailure>
            {
                new("Name", "Name is required"),
                new("TransactionType", "TransactionType is required")
            };

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var result = await _handler.Handle(
                new CreateCategoryCommand(dto, _userId), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.ValidationFailed);
        }

        [Fact]
        public async Task Handle_AlreadyExists_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var existingCategory = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.Name, "Groceries")
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(existingCategory, ct);
            await _db.SaveChangesAsync(ct);

            var dto = Fakers.CategoryDtoFaker()
                .RuleFor(c => c.Name, "Groceries")
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(
                new CreateCategoryCommand(dto, _userId), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.CategoryAlreadyExists);
        }
    }
}
