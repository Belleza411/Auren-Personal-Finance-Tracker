using Auren.API.Data;
using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace Auren.API.Repositories.Implementations
{
	public class TransactionRepository : ITransactionRepository
	{
		private readonly ILogger<TransactionRepository> _logger;
		private readonly AurenDbContext _dbContext;
        private readonly string _connection;

        public TransactionRepository(ILogger<TransactionRepository> logger, AurenDbContext dbContext, IConfiguration configuration)
		{
			_logger = logger;
			_dbContext = dbContext;
            _connection = configuration.GetConnectionString("AurenDbConnection") ?? throw new ArgumentNullException("Connection string not found.");
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
                var category = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.Name == transactionDto.Category && c.UserId == userId, cancellationToken);

                if (category == null)
                {
                    _logger.LogWarning("Category '{CategoryName}' not found for user {UserId}", transactionDto.Category, userId);
                    throw new ArgumentException("Category not found for the user.");
                }

                if(transactionDto.TransactionType != category.TransactionType)
                {
                    throw new ArgumentException("Transaction type must match the category's transaction type.");
                }


                if(transactionDto.TransactionType == TransactionType.Expense)
                {
                    var currentBalance = await GetBalanceAsync(userId, cancellationToken);
                    
                    if (currentBalance < transactionDto.Amount)
                    {
                        _logger.LogWarning(
                            "Insufficient funds for user {UserId}. Tried to create expense {Amount} with balance {Balance}",
                            userId,
                            transactionDto.Amount,
                            currentBalance
                        );

                        throw new InvalidOperationException("Insufficient funds for this transaction.");
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

		public async Task<AvgDailySpendingResponse> GetAvgDailySpendingAsync(Guid userId, DateTime month, CancellationToken cancellationToken)
		{
			
            async Task<decimal> GetMonthlyAvg(DateTime date)
            {
                int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endDate = startDate.AddMonths(1);

                var query = @"
                    SELECT 
                        SUM(CAST(Amount AS DECIMAL(18,2)))
                    FROM 
                        Transactions
                    WHERE 
                        UserId = @UserId
                        AND TransactionType = @TransactionType
                        AND TransactionDate >= @StartDate
                        AND TransactionDate < @EndDate";

                await using var connection = new SqlConnection(_connection);
                await connection.OpenAsync(cancellationToken);

                var total = await connection.ExecuteScalarAsync<decimal?>(query,
                    new
                    {
                        UserId = userId,
                        TransactionType = (int)TransactionType.Expense,
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    commandTimeout: 0,
                    commandType: null
                ) ?? 0;

                return total / daysInMonth;
            }

            var currentAvg = await GetMonthlyAvg(month);
            var lastMonthAvg = await GetMonthlyAvg(month.AddMonths(-1));

            decimal percentageChange = 0;
            if(lastMonthAvg > 0)
            {
                percentageChange = ((currentAvg - lastMonthAvg) / lastMonthAvg) * 100;
            }

            return new AvgDailySpendingResponse(currentAvg, percentageChange);
        }

		public async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
		{
            var result = await _dbContext.Transactions
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.TransactionType)
                .Select(g => new
                {
                    Type = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToListAsync(cancellationToken);

            var income = result.FirstOrDefault(r => r.Type == TransactionType.Income)?.Total ?? 0;
            var expense = result.FirstOrDefault(r => r.Type == TransactionType.Expense)?.Total ?? 0;

            return income - expense;
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

		public async Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid userId,
            CancellationToken cancellationToken,
            TransactionFilter filter,
            int? pageSize = 5, int? pageNumber = 1)
		{
			try
			{
                var skip = ((pageNumber ?? 1) - 1) * (pageSize ?? 5);

                var query = _dbContext.Transactions
                    .Where(t => t.UserId == userId);

                if(HasActiveFilters(filter))
                {
                    query = ApplyTransactionFilters(query, filter, userId);
                }

                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip(skip)
                    .Take(pageSize ?? 5)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} transactions for user {UserId}",
                    transactions.Count, userId);

                return transactions;

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

                var category = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.Name == transactionDto.Category && c.UserId == userId, cancellationToken);

                if (category == null)
                {
                    _logger.LogWarning("Category '{CategoryName}' not found for user {UserId}", transactionDto.Category, userId);
                    return null;
                }

                if(category.TransactionType != transactionDto.TransactionType)
                {
                    _logger.LogWarning("Transaction type '{TransactionType}' does not match category's transaction type for user {UserId}", transactionDto.TransactionType, userId);
                    return null;
                }

                transaction.Name = transactionDto.Name;
                transaction.Amount = transactionDto.Amount;
                transaction.PaymentType = transactionDto.PaymentType;
                transaction.TransactionType = category.TransactionType;
                transaction.CategoryId = category.CategoryId;

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

        private IQueryable<Transaction> ApplyTransactionFilters(IQueryable<Transaction> query, TransactionFilter filter, Guid userId)
        {
            if (filter == null) return query;

            if(filter.IsIncome == true)
                query = query.Where(t => t.TransactionType == TransactionType.Income);
            
            if (filter.IsExpense == true)
                query = query.Where(t => t.TransactionType == TransactionType.Expense);

            if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                query = query.Where(t => t.TransactionDate >= filter.StartDate && t.TransactionDate <= filter.EndDate);
            
            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);
            
            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);          

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(t => _dbContext.Categories
                    .Where(c => c.CategoryId == t.CategoryId
                    && c.Name.Contains(filter.Category)
                    && c.UserId == userId).Any()
                );
            }

            if(!string.IsNullOrEmpty(filter.PaymentMethod))
            {
                if(Enum.TryParse<PaymentType>(filter.PaymentMethod, true, out var paymentType))
                {
                    query = query.Where(t => t.PaymentType == paymentType);
                }
            }

            return query;
        }

        private bool HasActiveFilters(TransactionFilter filter)
        {
            if (filter == null) return false;

            return filter.IsIncome.HasValue ||
                   filter.IsExpense.HasValue ||
                   filter.MinAmount.HasValue ||
                   filter.MaxAmount.HasValue ||
                   filter.StartDate.HasValue ||
                   filter.EndDate.HasValue ||
                   !string.IsNullOrEmpty(filter.Category) ||
                   !string.IsNullOrEmpty(filter.PaymentMethod);
        }
    }
}

