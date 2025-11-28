using Auren.API.Data;
using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.Extensions;
using Auren.API.Helpers.Result;
using Auren.API.Repositories.Interfaces;
using Auren.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Transactions;

namespace Auren.API.Controllers
{
	[Route("api/transactions")]
	[ApiController]
    [Authorize]
	public class TransactionsController : ControllerBase
	{
		private readonly ILogger<TransactionsController> _logger;
        private readonly ITransactionService _transactionService;

		public TransactionsController(ILogger<TransactionsController> logger, ITransactionService transactionService)
		{
			_logger = logger;
			_transactionService = transactionService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransaction(
            [FromQuery] TransactionFilter transactionFilter,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
			{
                var transactions = await _transactionService.GetAllTransactions(userId.Value, transactionFilter, pageSize, pageNumber, cancellationToken);

                return Ok(transactions.Value);
            }
			catch (Exception ex)
			{
                _logger.LogError(ex, "Failed to retrieved transactions for {UserId}", userId);
				return StatusCode(500, "An error occurred while retrieving transactions. Please try again later.");
            }
        }

		[HttpGet("{transactionId:guid}")]
		public async Task<ActionResult<Transaction>> GetTransactionById([FromRoute] Guid transactionId, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
			{
                var transaction = await _transactionService.GetTransactionById(transactionId, userId.Value, cancellationToken);

                return transaction.IsSuccess ? Ok(transaction.Value) : NotFound(transaction?.Error);
            } 
			catch (Exception ex)
			{
                _logger.LogError(ex,
                    "Error retrieving transaction {TransactionId} for user {UserId}",
                    transactionId, userId);
                return StatusCode(500, "An error occurred while retrieving the transaction.");
            }
        }

		[HttpPost]
		public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] TransactionDto transactionDto, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var createdTransaction = await _transactionService.CreateTransaction(transactionDto, userId.Value, cancellationToken);

            if (!createdTransaction.IsSuccess)
            {
                return createdTransaction.Error.Code switch
                {
                    ErrorType.InvalidInput
                        or ErrorType.ValidationFailed
                        or ErrorType.TypeMismatch
                        or ErrorType.NotEnoughBalance
                            => BadRequest(createdTransaction.Error),
            
                    ErrorType.NotFound => NotFound(createdTransaction.Error),
                    ErrorType.CreateFailed => StatusCode(500, createdTransaction.Error),
                   
                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }

            return CreatedAtAction(nameof(GetTransactionById), new { transactionId = createdTransaction.Value.TransactionId }, createdTransaction.Value);
        }

		[HttpPut("{transactionId:guid}")]
		public async Task<ActionResult<Transaction>> UpdateTransaction([FromRoute] Guid transactionId, [FromBody] TransactionDto transactionDto, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

			try
			{
                var updatedTransaction = await _transactionService.UpdateTransaction(transactionId, userId.Value, transactionDto, cancellationToken);

                if (!updatedTransaction.IsSuccess)
                {
                    return updatedTransaction.Error.Code switch
                    {
                        ErrorType.InvalidInput
                            or ErrorType.ValidationFailed
                            or ErrorType.TypeMismatch 
                                => BadRequest(updatedTransaction.Error),

                        ErrorType.NotFound => NotFound(updatedTransaction.Error),
                        ErrorType.UpdateFailed=> StatusCode(500, updatedTransaction.Error),

                        _ => StatusCode(500, "An unexpected error occurred.")
                    };
                }

                return updatedTransaction != null ? Ok(updatedTransaction.Value) : NotFound(updatedTransaction?.Error);   
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId} for user {UserId}", transactionId, userId);
                return StatusCode(500, "An error occurred while updating the transaction.");
            }
        }

		[HttpDelete("{transactionId:guid}")]
		public async Task<IActionResult> DeleteTransaction([FromRoute] Guid transactionId, CancellationToken cancellationToken)  
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
			{
                var success = await _transactionService.DeleteTransaction(transactionId, userId.Value, cancellationToken);

                return success.IsSuccess ? NoContent() : NotFound($"Transaction with ID {transactionId} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", transactionId, userId);
                return StatusCode(500, "An error occurred while deleting the transaction.");
            }
        }
    }
}
