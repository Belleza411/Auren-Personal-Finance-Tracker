using Auren.Application.Common.Result;
using Auren.Application.Features.Transactions.Commands.CreateTransaction;
using Auren.Application.Features.Transactions.DTOs;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Auren.Tests.Features.Transactions.Commands
{
    public class CreateTransactionHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly Mock<IValidator<TransactionDto>> _validator = new();
        private readonly CreateTransactionHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly ITestOutputHelper _output;

        public CreateTransactionHandlerTests(ITestOutputHelper output)
        {
            _output = output;
            _handler = new CreateTransactionHandler(_db, _validator.Object);
        }

        [Fact]
        public async Task Handle_ValidTransaction_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var income = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.Name, "Freelance Salary Credit")
                .RuleFor(t => t.TransactionType, TransactionType.Income)
                .RuleFor(t => t.Amount, 5000)
                .Generate();

            await _db.Transactions.AddAsync(income, ct);
            await _db.SaveChangesAsync(ct);

            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Category, category.Name)
                .RuleFor(t => t.TransactionType, category.TransactionType)
                .RuleFor(t => t.Amount, 100)
                .Generate();

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(new CreateTransactionCommand(_userId, dto), ct);

            result.IsSuccess.Should().BeTrue();
            result.Value.Amount.Should().Be(dto.Amount);
            result.Value.UserId.Should().Be(_userId);
        }

        [Fact] 
        public async Task Handle_NotEnoughBalance_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Category, category.Name)
                .RuleFor(t => t.TransactionType, category.TransactionType)
                .RuleFor(t => t.Amount, 100)
                .Generate();

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(new CreateTransactionCommand(_userId, dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotEnoughBalance);
        }

        [Fact]
        public async Task Handle_TypeMismatch_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);

            var dto = Fakers.TransactionDtoFaker()
               .RuleFor(t => t.Category, category.Name)
               .RuleFor(t => t.TransactionType, TransactionType.Expense)
               .RuleFor(t => t.Amount, 100)
               .Generate();

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(new CreateTransactionCommand(_userId, dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.TypeMismatch);
        }
    }
}
