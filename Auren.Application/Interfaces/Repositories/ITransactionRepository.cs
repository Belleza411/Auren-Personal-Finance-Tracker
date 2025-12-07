
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Domain.Entities;
using Auren.Domain.Enums;

namespace Auren.Application.Interfaces.Repositories
{
	public interface ITransactionRepository
	{
		Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid userId, TransactionFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
		Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken);
		Task<Transaction> CreateTransactionAsync(Transaction transaction, Guid userId, CancellationToken cancellationToken);
		Task<Transaction?> UpdateTransactionAsync(Guid transactionId, Guid userId, Transaction transaction, CancellationToken cancellationToken);
		Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken);
		Task<decimal> GetBalanceAsync(Guid userId, BalancePeriod balancePeriod, CancellationToken cancellationToken);
		Task<AvgDailySpendingResponse> GetAvgDailySpendingAsync(Guid userId, CancellationToken cancellationToken);
		Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid userId, TimePeriod? timePeriod, CancellationToken cancellationToken);
    }
}
