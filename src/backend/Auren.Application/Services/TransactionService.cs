using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using Auren.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auren.Application.Services
{
	public class TransactionService : ITransactionService
	{
        private readonly ILogger<TransactionService> _logger;
        private readonly IValidator<TransactionDto> _validator;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

		public TransactionService(ILogger<TransactionService> logger, IValidator<TransactionDto> validator, ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
		{
			_logger = logger;
			_validator = validator;
			_transactionRepository = transactionRepository;
			_categoryRepository = categoryRepository;
		}

		public async Task<Result<Transaction>> CreateTransaction(TransactionDto transactionDto, Guid userId, CancellationToken cancellationToken)
		{
            if (transactionDto == null)
                return Result.Failure<Transaction>(Error.InvalidInput("All fields are required. "));
            
            var validationResult = await _validator.ValidateAsync(transactionDto, cancellationToken);
			if(!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
				return Result.Failure<Transaction>(Error.ValidationFailed(errors));
            }

            var category = await _categoryRepository.GetCategoryByNameAsync(
                userId,
                new CategoryDto(transactionDto.Category, transactionDto.TransactionType),
                cancellationToken
            );

            if (category == null)    
                return Result.Failure<Transaction>(Error.NotFound("Category not found. "));

            if (transactionDto.TransactionType != category.TransactionType)
                return Result.Failure<Transaction>(Error.TypeMismatch("Transaction type does not match category type."));
            

            if (transactionDto.TransactionType == TransactionType.Expense)
            {
                var currentBalance = await _transactionRepository.GetBalanceAsync(userId, DateTime.MinValue, DateTime.Today, cancellationToken);

                if (currentBalance.Balance < transactionDto.Amount)
                {
                    _logger.LogWarning(
                        "Insufficient funds for user {UserId}. Tried to create expense {Amount} with balance {Balance}",
                        userId,
                        transactionDto.Amount,
                        currentBalance
                    );

                    return Result.Failure<Transaction>(Error.NotEnoughBalance($"{currentBalance} is not enough. "));
                }
            }

            var transaction = new Transaction
            {
                TransactionId = Guid.NewGuid(),
                UserId = userId,
                CategoryId = category.CategoryId,
                TransactionType = category.TransactionType,
                Name = transactionDto.Name,
                Amount = transactionDto.Amount,
                PaymentType = transactionDto.PaymentType,
                CreatedAt = DateTime.UtcNow
            };

            var createdTransaction = await _transactionRepository.CreateTransactionAsync(transaction, userId, cancellationToken);
            return createdTransaction != null 
                ? Result.Success<Transaction>(transaction)
                : Result.Failure<Transaction>(Error.CreateFailed("Failed to create transaction. "));
        }

        public async Task<Result<bool>> DeleteTransaction(Guid transactionId, Guid userId, CancellationToken cancellationToken)
        {
            var deletedTransaction = await _transactionRepository.DeleteTransactionAsync(transactionId, userId, cancellationToken);

            return deletedTransaction
                ? Result.Success(true)
                : Result.Failure<bool>(Error.NotFound("Transaction not found. "));
        }
        
        public async Task<Result<decimal>> GetBalance(Guid userId, TimePeriod timePeriod, CancellationToken cancellationToken)
        {
            var (startDate, endDate) = timePeriod switch
            {
                TimePeriod.Last3Months => DateTime.Today.GetLast3MonthRange(),
                TimePeriod.Last6Months => DateTime.Today.GetLast6MonthRange(),
                TimePeriod.ThisYear => DateTime.Today.GetThisYearRange(),
                TimePeriod.LastMonth => DateTime.Today.GetLastMonthRange(),
                TimePeriod.ThisMonth => DateTime.Today.GetCurrentMonthRange(),
                TimePeriod.AllTime => (DateTime.MinValue, DateTime.Today),
                _ => (DateTime.MinValue, DateTime.Today)
            };

            var balance = await _transactionRepository.GetBalanceAsync(userId, startDate, endDate, cancellationToken);
            return Result.Success<decimal>(balance.Balance);
        }
            

        public async Task<Result<Transaction?>> GetTransactionById(Guid transactionId, Guid userId, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId, userId, cancellationToken);
            return transaction != null
                ? Result.Success<Transaction?>(transaction)
                : Result.Failure<Transaction?>(Error.NotFound("Transaction not found. "));
        }

        public async Task<Result<IEnumerable<Transaction>>> GetAllTransactions(Guid userId,
            TransactionFilter filter,
            int pageSize = 5, int pageNumber = 1,
            CancellationToken cancellationToken = default) 
                => Result.Success(await _transactionRepository.GetTransactionsAsync(userId, filter, pageSize, pageNumber, cancellationToken));

        public async Task<Result<Transaction>> UpdateTransaction(Guid transactionId, Guid userId, TransactionDto transactionDto, CancellationToken cancellationToken)
        {
            if (transactionDto == null)
                return Result.Failure<Transaction>(Error.InvalidInput("All fields are required. "));
            

            var validationResult = await _validator.ValidateAsync(transactionDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<Transaction>(Error.ValidationFailed(errors));
            }

            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId, userId, cancellationToken);

            if (transaction == null)
                return Result.Failure<Transaction>(Error.NotFound("Transaction not found. "));

            var category = await _categoryRepository.GetCategoryByNameAsync(
                userId,
                new CategoryDto(transactionDto.Category, transactionDto.TransactionType),
                cancellationToken
            );

            if (category == null)
                return Result.Failure<Transaction>(Error.NotFound("Category not found. "));
            

            if (transactionDto.TransactionType != category.TransactionType)
                return Result.Failure<Transaction>(Error.TypeMismatch("Transaction type does not match category type."));
            
            transaction.Name = transactionDto.Name;
            transaction.Amount = transactionDto.Amount;
            transaction.PaymentType = transactionDto.PaymentType;
            transaction.TransactionType = category.TransactionType;
            transaction.CategoryId = category.CategoryId;

            var updatedTransaction = await _transactionRepository.UpdateTransactionAsync(transactionId, userId, transaction, cancellationToken);

            return updatedTransaction != null
                ? Result.Success<Transaction>(updatedTransaction)
                : Result.Failure<Transaction>(Error.UpdateFailed("Failed to update transaction. "));
        }

        public async Task<Result<DashboardSummaryResponse>> GetDashboardSummary(Guid userId, TimePeriod timePeriod = TimePeriod.ThisMonth, CancellationToken cancellationToken = default)
            => Result.Success(await _transactionRepository.GetDashboardSummaryAsync(userId, timePeriod, cancellationToken));

        public async Task<Result<decimal>> GetAvgDailySpending(Guid userId, TimePeriod timePeriod, CancellationToken cancellationToken)
        {
            var (startDate, endDate) = timePeriod switch
            {
                TimePeriod.Last3Months => DateTime.Today.GetLast3MonthRange(),
                TimePeriod.Last6Months => DateTime.Today.GetLast6MonthRange(),
                TimePeriod.ThisYear => DateTime.Today.GetThisYearRange(),
                TimePeriod.LastMonth => DateTime.Today.GetLastMonthRange(),
                TimePeriod.ThisMonth => DateTime.Today.GetCurrentMonthRange(),
                TimePeriod.AllTime => (DateTime.MinValue, DateTime.Today),
                _ => (DateTime.MinValue, DateTime.Today)
            };

            var expenses = await _transactionRepository.GetExpensesAsync(userId, startDate, endDate, cancellationToken);

            if (expenses is null)
                return Result.Success(0m);

            var totalSpending = expenses.Sum(e => e.Amount);
            var totalDays = (endDate - startDate).TotalDays + 1;

            if (totalDays <= 0)
                return Result.Success(0m);

            var avg = totalSpending / (decimal)totalDays;

            return Result.Success(avg);
        }
    }
}
