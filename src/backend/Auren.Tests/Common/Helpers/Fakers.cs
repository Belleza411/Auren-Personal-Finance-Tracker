using Auren.Application.Features.Categories.DTOs;
using Auren.Application.Features.Transactions.DTOs;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using Bogus;

namespace Auren.Tests.Common.Helpers
{
    public static class Fakers
    {
        public static Faker<Transaction> TransactionFaker(Guid? userId = null) =>
            new Faker<Transaction>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.UserId, f => userId ?? Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Commerce.ProductName())
                .RuleFor(t => t.Amount, f => f.Finance.Amount(1, 1000))
                .RuleFor(t => t.TransactionType, f => f.PickRandom<TransactionType>())
                .RuleFor(t => t.PaymentType, f => f.PickRandom<PaymentType>())
                .RuleFor(t => t.TransactionDate, f => f.Date.Recent(30))
                .RuleFor(t => t.CreatedAt, f => DateTime.UtcNow);
        public static Faker<Category> CategoryFaker(Guid? userId = null) =>
           new Faker<Category>()
               .RuleFor(c => c.Id, f => Guid.NewGuid())
               .RuleFor(c => c.UserId, f => userId ?? Guid.NewGuid())
               .RuleFor(c => c.Name, f => f.Commerce.Department())
               .RuleFor(c => c.TransactionType, f => f.PickRandom<TransactionType>())
               .RuleFor(c => c.CreatedAt, f => DateTime.UtcNow);

        public static Faker<ApplicationUser> UserFaker() =>
           new Faker<ApplicationUser>()
               .RuleFor(u => u.Id, f => Guid.NewGuid())
               .RuleFor(u => u.Email, f => f.Internet.Email())
               .RuleFor(u => u.UserName, (f, u) => u.Email)
               .RuleFor(u => u.FirstName, f => f.Name.FirstName())
               .RuleFor(u => u.LastName, f => f.Name.LastName())
               .RuleFor(u => u.CreatedAt, f => DateTime.UtcNow);

        public static Faker<TransactionDto> TransactionDtoFaker() =>
            new Faker<TransactionDto>()
                .CustomInstantiator(f => new TransactionDto(
                    Name: f.Commerce.ProductName(),
                    Amount: f.Finance.Amount(1, 1000),
                    Category: f.Commerce.Department(),
                    TransactionType: f.PickRandom<TransactionType>(),
                    PaymentType: f.PickRandom<PaymentType>(),
                    TransactionDate: f.Date.Recent(30)
                ));

        public static Faker<CategoryDto> CategoryDtoFaker() => 
            new Faker<CategoryDto>()
                .CustomInstantiator(f => new CategoryDto(
                    Name: f.Commerce.Department(),
                    TransactionType: f.PickRandom<TransactionType>()
                ));
    }
}
