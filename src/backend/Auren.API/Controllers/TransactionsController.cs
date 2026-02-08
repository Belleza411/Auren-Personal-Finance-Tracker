using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.Transaction;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Transactions;

namespace Auren.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    [EnableRateLimiting("fixed")]
    [Authorize]
    public class TransactionsController(ITransactionService transactionService) : ControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<PagedResult<Transaction>>> GetAllTransaction(
            [FromQuery] TransactionFilter transactionFilter,
            [FromQuery] int pageSize = 10,
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
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                        or ErrorTypes.TypeMismatch
                        or ErrorTypes.NotEnoughBalance
                            => BadRequest(createdTransaction.Error),
            
                    ErrorTypes.NotFound => NotFound(createdTransaction.Error),
                    ErrorTypes.CreateFailed => StatusCode(500, createdTransaction.Error),
                   
                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }

            return CreatedAtAction(nameof(GetTransactionById), new { transactionId = createdTransaction.Value.Id }, createdTransaction.Value);
        }

		[HttpPut("{transactionId:guid}")]
		public async Task<ActionResult<Transaction>> UpdateTransaction(
            [FromRoute] Guid transactionId, 
            [FromBody] TransactionDto transactionDto, 
            CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var updatedTransaction = await transactionService.UpdateTransaction(transactionId, userId.Value, transactionDto, cancellationToken);

            if (!updatedTransaction.IsSuccess)
            {
                return updatedTransaction.Error.Code switch
                {
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                        or ErrorTypes.TypeMismatch 
                            => BadRequest(updatedTransaction.Error),

                    ErrorTypes.NotFound => NotFound(updatedTransaction.Error),
                    ErrorTypes.UpdateFailed=> StatusCode(500, updatedTransaction.Error),

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

        [HttpGet("average-daily-spending")]
        public async Task<ActionResult<AvgDailySpendingResponse>> GetAvgDailySpending([FromQuery] TimePeriod? timePeriod, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var avgSpending = await transactionService.GetAvgDailySpending(userId.Value, timePeriod ?? TimePeriod.AllTime, cancellationToken);

            return Ok(avgSpending.Value);
        }

        [HttpGet("balance")]
        public async Task<ActionResult<BalanceSummaryResponse>> GetUserBalance([FromQuery] TimePeriod? timePeriod, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var balance = await transactionService.GetBalance(userId.Value, timePeriod ?? TimePeriod.AllTime, cancellationToken);

            return Ok(balance.Value);
        }
    }
}
