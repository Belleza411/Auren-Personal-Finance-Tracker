using Auren.API.Data;
using Auren.API.DTOs.Requests;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace Auren.API.Repositories.Implementations
{
	public class TransactionRepository : ITransactionRepository
	{
		private readonly ILogger<TransactionRepository> _logger;
		private readonly AurenDbContext _dbContext;

		public TransactionRepository(ILogger<TransactionRepository> logger, AurenDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<Transaction> CreateTransactionAsync(TransactionDto transactionDto, Guid userId, CancellationToken cancellationToken)
		{
			if(transactionDto?.Category == null)
			{
                _logger.LogWarning("TransactionDto or Category is null for user {UserId}", userId);
                throw new ArgumentException("Transaction data and category are required");
            }

            try
			{
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = transactionDto.Category.CategoryId,
                    TransactionType = transactionDto.TransactionType,
                    Name = transactionDto.Name,
                    Amount = transactionDto.Amount,
                    PaymentType = transactionDto.PaymentType,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.Transactions.AddAsync(transaction, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
				_logger.LogInformation("Transaction created successfully for {UserId} with TransactionId of {TransactionId}. ", userId, transaction.TransactionId);

                return transaction;
            }
			catch (Exception ex)
			{
                _logger.LogError(ex, "Failed to create transaction for user {UserId}", userId);
                throw;
            }
        }

		public async Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
		{
			try
			{
                var transaction = await _dbContext.Transactions
					.FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId, cancellationToken);

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found for user {UserId}", transactionId, userId);
                    return false;
                }

                _dbContext.Transactions.Remove(transaction);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Transaction deleted successfully for {UserId} with TransactionId of {TransactionId}. ", userId, transaction.TransactionId);

                return true;
            }
			catch(Exception ex)
			{
				_logger.LogError(ex, "Failed to delete transaction for user {UserId} with TransactionId of {TransactionId}", userId, transactionId);
				return false;
            }
        }

		public async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
		{
			try
            {
                var income = await _dbContext.Transactions
                    .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Income)
                    .SumAsync(t => t.Amount, cancellationToken);

                var expense = await _dbContext.Transactions
                    .Where(t => t.UserId == userId && t.TransactionType == TransactionType.Expense)
                    .SumAsync(t => t.Amount, cancellationToken);

                return income - expense;
            }
            catch(InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to calculate balance for {UserId}", userId);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calculating balance for {UserId}", userId);
                throw;
            }
		}

		public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
		{
			try
			{
                var transaction = await _dbContext.Transactions
					.FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId, cancellationToken);

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found for user {UserId}", transactionId, userId);
                }

                return transaction;
            } 
			catch(Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve transaction for user {UserId} with TransactionId of {TransactionId}", userId, transactionId);
				throw;
            }

        }

		public async Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid userId, CancellationToken cancellationToken)
		{
			try
			{
                var transaction = await _dbContext.Transactions
					.Where(t => t.UserId == userId)
					.ToListAsync(cancellationToken);

                if (!transaction.Any())
                {
                    _logger.LogWarning("No transactions found for user {UserId}", userId);
                }

                return transaction;
            }
			catch(Exception ex)
			{
                _logger.LogError(ex, "Failed to retrieve transactions for user {UserId}", userId);
                throw;
            }
        }

		public async Task<Transaction?> UpdateTransactionAsync(Guid transactionId, Guid userId, TransactionDto transactionDto, CancellationToken cancellationToken)
		{
			if(transactionDto?.Category == null)
			{
                _logger.LogWarning("TransactionDto or Category is null for user {UserId}", userId);
                throw new ArgumentException("Transaction data and category are required");
            }

			try
            {
                var transaction = await _dbContext.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId, cancellationToken);

                if (transaction == null)
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found for user {UserId}", transactionId, userId);
                    return null;
                }

                transaction.Name = transactionDto.Name;
                transaction.Amount = transactionDto.Amount;
                transaction.PaymentType = transactionDto.PaymentType;
                transaction.TransactionType = transactionDto.TransactionType;
                transaction.CategoryId = transactionDto.Category.CategoryId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Transaction updated successfully for {UserId} with TransactionId of {TransactionId}. ", userId, transaction.TransactionId);

                return transaction;
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update transaction {TransactionId} for user {UserId}", transactionId, userId);
                throw;
            }
		}
	}
}
