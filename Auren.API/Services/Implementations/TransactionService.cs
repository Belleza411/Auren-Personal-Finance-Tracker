using Auren.API.Data;
using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Implementations;
using Auren.API.Repositories.Interfaces;
using Auren.API.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Auren.API.Services.Implementations
{
	public class TransactionService : ITransactionService
	{
        private readonly ILogger<TransactionService> _logger;
        private readonly IValidator<TransactionDto> _validator;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly AurenDbContext _dbContext;

		public TransactionService(ILogger<TransactionService> logger, IValidator<TransactionDto> validator, ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, AurenDbContext dbContext)
		{
			_logger = logger;
			_validator = validator;
			_transactionRepository = transactionRepository;
			_categoryRepository = categoryRepository;
			_dbContext = dbContext;
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
                var currentBalance = await _transactionRepository.GetBalanceAsync(userId, cancellationToken, true);

                if (currentBalance < transactionDto.Amount)
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

        public async Task<Result<AvgDailySpendingResponse>> GetAvgDailySpending(Guid userId, CancellationToken cancellationToken)
            => Result.Success(await _transactionRepository.GetAvgDailySpendingAsync(userId, cancellationToken));
        
        public async Task<Result<decimal>> GetBalance(Guid userId, CancellationToken cancellationToken, bool isCurrentMonth)
            => Result.Success(await _transactionRepository.GetBalanceAsync(userId, cancellationToken, isCurrentMonth));

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

        public async Task<Result<DashboardSummaryResponse>> GetDashboardSummary(Guid userId, CancellationToken cancellationToken)
            => Result.Success(await _transactionRepository.GetDashboardSummaryAsync(userId, cancellationToken));
    }
}
