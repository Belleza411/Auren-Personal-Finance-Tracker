using Auren.Application.Common.Result;
using Auren.Application.Features.Transactions.Commands.CreateTransaction;
using Auren.Application.Features.Transactions.Commands.UpdateTransaction;
using Auren.Application.Features.Transactions.DTOs;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Auren.Infrastructure.Persistence;
using Auren.Tests.Common.Helpers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace Auren.Tests.Features.Transactions.Commands
{
    public class UpdateTransactionHandlerTests
    {
        private readonly AurenDbContext _db = TestDbContextFactory.CreateAppDb();
        private readonly Mock<IValidator<TransactionDto>> _validator = new();
        private readonly UpdateTransactionHandler _handler;
        private readonly Guid _userId = Guid.NewGuid();

        public UpdateTransactionHandlerTests()
        {
            _handler = new UpdateTransactionHandler(_db, _validator.Object);
        }

        [Fact]
        public async Task Handle_ValidTransaction_ReturnsSuccess()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            var income = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.Name, "Freelance Salary Credit")
                .RuleFor(t => t.TransactionType, TransactionType.Income)
                .RuleFor(t => t.Amount, 5000)
                .Generate();

            var currentTransaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, category.TransactionType)
                .RuleFor(t => t.Amount, 100)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddRangeAsync([income, currentTransaction], ct);
            await _db.SaveChangesAsync(ct);

            var dto = new TransactionDto(
                Name: currentTransaction.Name,
                Amount: 150,
                Category: category.Name,
                TransactionType: category.TransactionType,
                PaymentType: currentTransaction.PaymentType,    
                TransactionDate: currentTransaction.TransactionDate
            );
        
            _validator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(
                new UpdateTransactionCommand(_userId, currentTransaction.Id, dto), ct);

            result.Value.Amount.Should().Be(150);         
            result.Value.Amount.Should().NotBe(100);       
            result.Value.Name.Should().Be(dto.Name);     
            result.Value.CategoryId.Should().Be(category.Id);
            result.Value.UserId.Should().Be(_userId);
        }

        [Fact]
        public async Task Handle_TransactionNotFound_ReturnFailure()
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
                .Generate();

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(
                new UpdateTransactionCommand(_userId, Guid.NewGuid(), dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
        }

        [Fact]
        public async Task Handle_CategoryNotFound_ReturnFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Expense)
                .Generate();

            var currentTransaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, category.TransactionType)
                .RuleFor(t => t.Amount, 100)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddAsync(currentTransaction, ct);
            await _db.SaveChangesAsync(ct);

            var dto = new TransactionDto(
                Name: currentTransaction.Name,
                Amount: 150,
                Category: "PakNSave",
                TransactionType: category.TransactionType,
                PaymentType: currentTransaction.PaymentType,
                TransactionDate: currentTransaction.TransactionDate
            );

            _validator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(
                new UpdateTransactionCommand(_userId, currentTransaction.Id, dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.NotFound);
        }

        [Fact]
        public async Task Handle_TypeMismatch_ReturnFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate();

            var currentTransaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, category.TransactionType)
                .RuleFor(t => t.Amount, 100)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddAsync(currentTransaction, ct);
            await _db.SaveChangesAsync(ct);

            var dto = new TransactionDto(
                Name: currentTransaction.Name,
                Amount: 150,
                Category: category.Name,
                TransactionType: TransactionType.Expense,
                PaymentType: currentTransaction.PaymentType,
                TransactionDate: currentTransaction.TransactionDate
            );

            _validator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _handler.Handle(
                new UpdateTransactionCommand(_userId, currentTransaction.Id, dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.TypeMismatch);
        }

        [Fact]
        public async Task Handle_ValidationFailed_ReturnsFailure()
        {
            var ct = TestContext.Current.CancellationToken;

            var category = Fakers.CategoryFaker(_userId)
                .RuleFor(c => c.TransactionType, TransactionType.Income)
                .Generate();

            var currentTransaction = Fakers.TransactionFaker(_userId)
                .RuleFor(t => t.CategoryId, category.Id)
                .RuleFor(t => t.TransactionType, category.TransactionType)
                .RuleFor(t => t.Amount, 100)
                .Generate();

            await _db.Categories.AddAsync(category, ct);
            await _db.Transactions.AddAsync(currentTransaction, ct);
            await _db.SaveChangesAsync(ct);

            var dto = Fakers.TransactionDtoFaker().Generate();

            var failures = new List<ValidationFailure>
            {
                new("Amount", "Transaction amount must be greater than zero.")
            };

            _validator
                .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            var result = await _handler.Handle(
                new UpdateTransactionCommand(_userId, currentTransaction.Id, dto), ct);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be(ErrorTypes.ValidationFailed);
        }
    }
}
