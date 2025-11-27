using Auren.API.DTOs.Requests;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;

namespace Auren.API.Services.Interfaces
{
	public interface ITransactionService
	{
        Task<Result<Transaction>> CreateTransactionAsync(TransactionDto transactionDto, Guid userId, CancellationToken cancellationToken);
    }
}
