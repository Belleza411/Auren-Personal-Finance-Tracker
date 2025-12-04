using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Services;
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
	public class TransactionsController(ITransactionService transactionService) : ControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Transaction>>> GetAllTransaction(
            [FromQuery] TransactionFilter transactionFilter,
            [FromQuery] int pageSize = 5,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var transactions = await transactionService.GetAllTransactions(userId.Value, transactionFilter, pageSize, pageNumber, cancellationToken);

            return Ok(transactions.Value);        
        }

		[HttpGet("{transactionId:guid}")]
		public async Task<ActionResult<Transaction>> GetTransactionById([FromRoute] Guid transactionId, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var transaction = await transactionService.GetTransactionById(transactionId, userId.Value, cancellationToken);

            return transaction.IsSuccess ? Ok(transaction.Value) : NotFound(transaction?.Error);
        }

		[HttpPost]
		public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] TransactionDto transactionDto, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var createdTransaction = await transactionService.CreateTransaction(transactionDto, userId.Value, cancellationToken);

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

            var updatedTransaction = await transactionService.UpdateTransaction(transactionId, userId.Value, transactionDto, cancellationToken);

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

		[HttpDelete("{transactionId:guid}")]
		public async Task<IActionResult> DeleteTransaction([FromRoute] Guid transactionId, CancellationToken cancellationToken)  
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var success = await transactionService.DeleteTransaction(transactionId, userId.Value, cancellationToken);

            return success.IsSuccess ? NoContent() : NotFound($"Transaction with ID {transactionId} not found.");
        }
    }
}
