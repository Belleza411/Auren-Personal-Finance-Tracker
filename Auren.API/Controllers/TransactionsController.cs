using Auren.API.Data;
using Auren.API.DTOs.Requests;
using Auren.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Transactions;

namespace Auren.API.Controllers
{
	[Route("api/transactions")]
	[ApiController]
	public class TransactionsController : ControllerBase
	{
		private readonly ILogger<TransactionsController> _logger;
		private readonly ITransactionRepository _transactionRepository;

		public TransactionsController(ILogger<TransactionsController> logger, ITransactionRepository transactionRepository)
		{
			_logger = logger;
			_transactionRepository = transactionRepository;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransaction(CancellationToken cancellationToken)
		{
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
			{
                var transactions = await _transactionRepository.GetTransactionsAsync(userId.Value, cancellationToken);

                return Ok(transactions);
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve transactions for user {UserId}", userId);
				return StatusCode(500, "An error occurred while retrieving transactions. Please try again later.");
            }
        }

		[HttpGet("{transactionId:guid}")]
		public async Task<ActionResult<Transaction>> GetTransactionById([FromRoute] Guid transactionId, CancellationToken cancellationToken)
		{
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
			{
                var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionId, userId.Value, cancellationToken);

                if (transaction == null)
                {
                    return NotFound($"Transaction with ID {transactionId} not found.");
                }

                return Ok(transaction);
            } 
			catch (Exception ex)
			{
                _logger.LogError(ex, "Error retrieving transaction {TransactionId} for user {UserId}", transactionId, userId);
                return StatusCode(500, "An error occurred while retrieving the transaction.");
            }
        }

		[HttpPost]
		public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] TransactionDto transactionDto, CancellationToken cancellationToken)
		{
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
            }

			try
			{
                var createdTransaction = await _transactionRepository.CreateTransactionAsync(transactionDto, userId.Value, cancellationToken);

                return CreatedAtAction(nameof(GetTransactionById), new { transactionId = createdTransaction.TransactionId }, createdTransaction);
            }
			catch (ArgumentException ex)
			{
                _logger.LogWarning(ex, "Invalid transaction data provided for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction for user {UserId}", userId);
                return StatusCode(500, "An error occurred while creating the transaction.");
            }
        }

		[HttpPut("{transactionId:guid}")]
		public async Task<ActionResult<Transaction>> UpdateTransaction([FromRoute] Guid transactionId, [FromBody] TransactionDto transactionDto, CancellationToken cancellationToken)
		{
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
            }

			try
			{
                var updatedTransaction = await _transactionRepository.UpdateTransactionAsync(transactionId, userId.Value, transactionDto, cancellationToken);

				if(updatedTransaction == null)
				{
					return NotFound($"Transaction with ID {transactionId} not found.");
				}

                return Ok(updatedTransaction);
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
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
			{
                var success = await _transactionRepository.DeleteTransactionAsync(transactionId, userId.Value, cancellationToken);

                if (!success)
                {
                    return NotFound($"Transaction with ID {transactionId} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction {TransactionId} for user {UserId}", transactionId, userId);
                return StatusCode(500, "An error occurred while deleting the transaction.");
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("User ID claim not found in token");
                return null;
            }

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            _logger.LogWarning("Invalid user ID format in claim: {UserIdClaim}", userIdClaim);
            return null;
        }
    }
}
