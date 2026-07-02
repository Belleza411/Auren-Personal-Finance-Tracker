using Auren.Application.Features.Categories.Validators;
using Auren.Domain.Enums;
using Auren.Tests.Common.Helpers;
using FluentValidation.TestHelper;

namespace Auren.Tests.Validators
{
    public class CategoryValidatorTests
    {
        private readonly CategoryValidator _validator = new();

        [Trait("CategoryValidator", "Name")]
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void CategoryValidator_NameIsEmpty_ShouldFailValidation(string name)
        {
            var dto = Fakers.CategoryDtoFaker()
                .RuleFor(c => c.Name, name)
                .Generate();

            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Trait("CategoryValidator", "Name")]
        [Fact]
        public void CategoryValidator_NameExceedsMaxLength_ShouldFailValidation()
        {
            var longName = new string('a', 101); 
            var dto = Fakers.CategoryDtoFaker()
                .RuleFor(c => c.Name, longName)
                .Generate();
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Trait("CategoryValidator", "TransactionType")]
        [Fact]
        public void CategoryValidator_TransactionTypeIsInvalid_ShouldFailValidation()
        {
            var dto = Fakers.CategoryDtoFaker()
                .RuleFor(c => c.TransactionType, (TransactionType)999) 
                .Generate();
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.TransactionType);
        }
    }
}
