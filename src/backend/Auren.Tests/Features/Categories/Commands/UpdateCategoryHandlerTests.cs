using Auren.Application.Common.Result;
using Auren.Application.Features.Categories.Commands.UpdateCategory;
using Auren.Application.Features.Categories.DTOs;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Auren.Tests.Features.Categories.Commands
{
    public class UpdateCategoryHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly Mock<IValidator<CategoryDto>> _validator = new();
        private readonly UpdateCategoryHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public UpdateCategoryHandlerTests()
        {
            _handler = new UpdateCategoryHandler(_db, _validator.Object);
        }

        [Fact]
        public async Task Handle_ValidCategory_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var dto = new CategoryDto(
                Name: "Updated Category Name",
                TransactionType: TransactionType.Expense
            );

            _validator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var result = await _handler.Handle(
                new UpdateCategoryCommand(category.Id, _userId, dto), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(dto.Name);
        }

        [Fact] 
        public async Task Handle_ValidationFailed_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var dto = new CategoryDto(
                Name: "", 
                TransactionType: TransactionType.Expense
            );

            var failures = new List<ValidationFailure>
            {
                new("Name", "Name is required")
            };

            _validator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));


            var result = await _handler.Handle(
                new UpdateCategoryCommand(category.Id, _userId, dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.ValidationFailed);
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var dto = new CategoryDto(
                Name: "Updated Category Name",
                TransactionType: TransactionType.Expense
            );

            _validator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(
                new UpdateCategoryCommand(Guid.NewGuid(), _userId, dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
        }
    }
}  
