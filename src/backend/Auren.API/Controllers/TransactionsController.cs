using Auren.Application.Common.Models;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Dashboard.DTOs;
using Auren.Application.Features.Transactions.Commands.CreateTransaction;
using Auren.Application.Features.Transactions.Commands.DeleteTransaction;
using Auren.Application.Features.Transactions.Commands.UpdateTransaction;
using Auren.Application.Features.Transactions.DTOs;
using Auren.Application.Features.Transactions.Queries.GetBalance;
using Auren.Application.Features.Transactions.Queries.GetTransactionById;
using Auren.Application.Features.Transactions.Queries.GetTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Transactions;

namespace Auren.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    [Authorize]
    public class TransactionsController(
        CreateTransactionHandler createHandler,
        UpdateTransactionHandler updateHandler,
        DeleteTransactionHandler deleteHandler,
        GetTransactionsHandler getHandler,
        GetTransactionByIdHandler getByIdHandler,
        GetBalanceHandler getBalanceHandler) : ControllerBase
	{
		[HttpGet]
        [EnableRateLimiting("read")]
		public async Task<ActionResult<PagedResult<Transaction>>> GetAllTransaction(
            [FromQuery] TransactionFilter transactionFilter,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            CancellationToken ct = default)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = new GetTransactionsQuery(userId.Value, transactionFilter, pageNumber, pageSize);

            var transactions = await getHandler.Handle(query, ct);

            return Ok(transactions.Value);        
        }

		[HttpGet("{transactionId:guid}")]
        [EnableRateLimiting("read")]
        public async Task<ActionResult<Transaction>> GetTransactionById(
            Guid transactionId,
            CancellationToken ct)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = new GetTransactionByIdQuery(transactionId, userId.Value);

            var transaction = await getByIdHandler.Handle(query, ct);

            return transaction.IsSuccess ? Ok(transaction.Value) : NotFound(transaction?.Error);
        }

		[HttpPost]
        [EnableRateLimiting("sensitive")]
        public async Task<ActionResult<Transaction>> CreateTransaction(
            TransactionDto transactionDto,
            CancellationToken ct)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new CreateTransactionCommand(userId.Value, transactionDto);

            var createdTransaction = await createHandler.Handle(cmd, ct);

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
        [EnableRateLimiting("sensitive")]
		public async Task<ActionResult<Transaction>> UpdateTransaction(
            Guid transactionId, 
            TransactionDto transactionDto, 
            CancellationToken ct)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new UpdateTransactionCommand(userId.Value, transactionId, transactionDto);

            var updatedTransaction = await updateHandler.Handle(cmd, ct);

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
        [EnableRateLimiting("write")]
		public async Task<IActionResult> DeleteTransaction(
            Guid transactionId,
            CancellationToken ct)  
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new DeleteTransactionCommand(userId.Value, transactionId);

            var success = await deleteHandler.Handle(cmd, ct);

            return success.IsSuccess ? NoContent() : NotFound($"Transaction with ID {transactionId} not found.");
        }

        [HttpGet("balance")]
        [EnableRateLimiting("sensitive")]
        public async Task<ActionResult<BalanceSummaryResponse>> GetUserBalance(CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();


            var balance = await getBalanceHandler.Handle(new GetBalanceQuery(userId.Value), ct);

            return Ok(balance.Value);
        }
    }
}
