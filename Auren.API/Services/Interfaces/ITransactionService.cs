using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;

namespace Auren.API.Services.Interfaces
{
	public interface ITransactionService
	{
        Task<Result<Transaction>> CreateTransaction(TransactionDto transactionDto, Guid userId, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteTransaction(Guid transactionId, Guid userId, CancellationToken cancellationToken);
        Task<Result<AvgDailySpendingResponse>> GetAvgDailySpending(Guid userId, CancellationToken cancellationToken);
        Task<Result<decimal>> GetBalance(Guid userId, CancellationToken cancellationToken, bool isCurrentMonth);
        Task<Result<Transaction?>> GetTransactionById(Guid transactionId, Guid userId, CancellationToken cancellationToken);
        Task<Result<IEnumerable<Transaction>>> GetAllTransactions(Guid userId, TransactionFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
        Task<Result<Transaction>> UpdateTransaction(Guid transactionId, Guid userId, TransactionDto transactionDto, CancellationToken cancellationToken);
        Task<Result<DashboardSummaryResponse>> GetDashboardSummary(Guid userId, CancellationToken cancellationToken);
    }
}
