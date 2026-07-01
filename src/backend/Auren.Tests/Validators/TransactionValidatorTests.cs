using Auren.Application.Features.Transactions.Validators;
using Auren.Domain.Enums;
using Auren.Tests.Common.Helpers;
using FluentValidation.TestHelper;

namespace Auren.Tests.Validators
{
    public class TransactionValidatorTests
    {
        private readonly TransactionValidator _validator = new();

        [Trait("TransactionValidator", "Amount")]
        [Theory]
        [InlineData(10.123)]
        [InlineData(25.1234)]
        [InlineData(50.99999)]
        public void TransactionValidator_AmountWithMoreThanTwoDecimalPlaces_ShouldFailValidation(decimal amount)
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Amount, amount)
                .Generate();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Trait("TransactionValidator", "Amount")]
        [Theory]
        [InlineData(10.20)]
        [InlineData(15.1)]
        [InlineData(25.12)]
        public void TransactionValidator_AmountWithTwoOrFewerDecimalPlaces_ShouldPassValidation(decimal amount)
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Amount, amount)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [Trait("TransactionValidator", "Amount")]
        [Fact]
        public void TransactionValidator_AmountZero_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Amount, 0)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Trait("TransactionValidator", "Amount")]
        [Fact]
        public void TransactionValidator_AmountNegative_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Amount, -10)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void TransactionValidator_ValidTransaction_ShouldPassValidation()
        {
            var dto = Fakers.TransactionDtoFaker().Generate();
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Trait("TransactionValidator", "Name")]
        [Fact]
        public void TransactionValidator_EmptyName_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Name, string.Empty)
                .Generate();

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Trait("TransactionValidator", "Name")]
        [Fact]
        public void TransactionValidator_NameExceedsMaxLength_ShouldFailValidation()
        {
            var longName = new string('A', 101);
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Name, longName)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Trait("TransactionValidator", "Category")]
        [Fact]
        public void TransactionValidator_EmptyCategory_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.Category, string.Empty)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Category);
        }

        [Trait("TransactionValidator", "TransactionType")]
        [Fact]
        public void TransactionValidator_EmptyTransactionType_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
               .RuleFor(t => t.TransactionType, (TransactionType)0)
               .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.TransactionType);
        }

        [Trait("TransactionValidator", "TransactionType")]
        [Fact]
        public void TransactionValidator_InvalidTransactionType_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.TransactionType, (TransactionType)999)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.TransactionType);
        }

        [Trait("TransactionValidator", "PaymentType")]
        [Fact]
        public void TransactionValidator_EmptyPaymentType_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
               .RuleFor(t => t.PaymentType, (PaymentType)0)
               .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.PaymentType);
        }

        [Trait("TransactionValidator", "PaymentType")]
        [Fact]
        public void TransactionValidator_InvalidPaymentType_ShouldFailValidation()
        {
            var dto = Fakers.TransactionDtoFaker()
                .RuleFor(t => t.PaymentType, (PaymentType)999)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.PaymentType);
        }
    }
}
