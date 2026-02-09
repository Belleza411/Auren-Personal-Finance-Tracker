
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Domain.Entities;
using Auren.Domain.Enums;

namespace Auren.Application.Interfaces.Repositories
{
	public interface ITransactionRepository : IRepository<Transaction>
	{
		Task<PagedResult<Transaction>> GetTransactionsAsync(Guid userId, TransactionFilter filter, int pageSize = 5, int pageNumber = 1, CancellationToken cancellationToken = default);
		Task<BalanceSummaryResponse> GetBalanceAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken);
    }
}
