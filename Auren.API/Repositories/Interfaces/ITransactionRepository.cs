using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Auren.API.Repositories.Interfaces
{
	public interface ITransactionRepository
	{
		Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid userId, CancellationToken cancellationToken, TransactionFilter filter, int? pageSize = 5, int? pageNumber = 1);
		Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken);
		Task<Transaction> CreateTransactionAsync(TransactionDto transactionDto, Guid userId, CancellationToken cancellationToken);
		Task<Transaction?> UpdateTransactionAsync(Guid transactionId, Guid userId, TransactionDto transactionDto, CancellationToken cancellationToken);
		Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken);
		Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken);
		Task<AvgDailySpendingResponse> GetAvgDailySpendingAsync(Guid userId, DateTime month, CancellationToken cancellationToken);
    }
}
