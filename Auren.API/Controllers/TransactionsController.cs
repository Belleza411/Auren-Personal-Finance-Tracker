using Auren.API.Data;
using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.Extensions;
using Auren.API.Repositories.Interfaces;
using Auren.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
		private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionService _transactionService;

		public TransactionsController(ILogger<TransactionsController> logger, ITransactionRepository transactionRepository, ITransactionService transactionService)
		{
			_logger = logger;
			_transactionRepository = transactionRepository;
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
                var transactions = await _transactionRepository.GetTransactionsAsync(userId.Value, transactionFilter, pageSize, pageNumber, cancellationToken);

                return Ok(transactions);
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
                var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId, userId.Value, cancellationToken);

                return transaction != null ? Ok(transaction) : NotFound("Transaction not found.");
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

            var createdTransaction = await _transactionService.CreateTransactionAsync(transactionDto, userId.Value, cancellationToken);

            return CreatedAtAction(nameof(GetTransactionById), new { transactionId = createdTransaction.Value.TransactionId }, createdTransaction);
        }

		[HttpPut("{transactionId:guid}")]
		public async Task<ActionResult<Transaction>> UpdateTransaction([FromRoute] Guid transactionId, [FromBody] TransactionDto transactionDto, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

			try
			{
                var updatedTransaction = await _transactionRepository.UpdateTransactionAsync(transactionId, userId.Value, transactionDto, cancellationToken);

                return updatedTransaction != null ? Ok(updatedTransaction) : NotFound($"Transaction with ID {transactionId} not found.");   
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid transaction data provided for update of transaction {TransactionId} for user {UserId}", transactionId, userId);
                return BadRequest(ex.Message);
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
                var success = await _transactionRepository.DeleteTransactionAsync(transactionId, userId.Value, cancellationToken);

                return success ? NoContent() : NotFound($"Transaction with ID {transactionId} not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", transactionId, userId);
                return StatusCode(500, "An error occurred while deleting the transaction.");
            }
        }
    }
}
