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
        public class TransactionsController : ControllerBase
	    {
		    [HttpGet]
            [EnableRateLimiting("read")]
		    public async Task<ActionResult<PagedResult<Transaction>>> GetAllTransaction(
                [FromServices] GetTransactionsHandler handler,
                [FromQuery] TransactionFilter transactionFilter,
                [FromQuery] int pageSize = 10,
                [FromQuery] int pageNumber = 1,
                CancellationToken ct = default)
		    {
                var userId = User.GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var query = new GetTransactionsQuery(userId.Value, transactionFilter, pageNumber, pageSize);

                var transactions = await handler.Handle(query, ct);

                return Ok(transactions.Value);        
            }

		    [HttpGet("{transactionId:guid}")]
            [EnableRateLimiting("read")]
            public async Task<ActionResult<Transaction>> GetTransactionById(
                [FromServices] GetTransactionByIdHandler handler,
                Guid transactionId,
                CancellationToken ct)
		    {
                var userId = User.GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var query = new GetTransactionByIdQuery(transactionId, userId.Value);

                var result = await handler.Handle(query, ct);

                return result.Match<ActionResult<Transaction>>(
                    onSuccess: value => Ok(value),
                    onFailure: _ => NotFound($"Transaction with ID {transactionId} not found."));
            }

		    [HttpPost]
            [EnableRateLimiting("sensitive")]
            public async Task<ActionResult<Transaction>> CreateTransaction(
                [FromServices] CreateTransactionHandler handler,
                TransactionDto transactionDto,
                CancellationToken ct)
		    {
                var userId = User.GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var cmd = new CreateTransactionCommand(userId.Value, transactionDto);

                var result = await handler.Handle(cmd, ct);

                return result.Match(
                    onSuccess: value => CreatedAtAction(nameof(GetTransactionById), new { transactionId = value.Id }, value),
                    onFailure: err => err.Code switch
                    {
                        ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                        or ErrorTypes.TypeMismatch
                        or ErrorTypes.NotEnoughBalance
                            => BadRequest(err),

                        ErrorTypes.NotFound => NotFound(err),
                        ErrorTypes.CreateFailed => StatusCode(500, err),

                        _ => StatusCode(500, "An unexpected error occurred.")
                    });
            }

		    [HttpPut("{transactionId:guid}")]
            [EnableRateLimiting("sensitive")]
		    public async Task<ActionResult<Transaction>> UpdateTransaction(
                [FromServices] UpdateTransactionHandler handler,
                Guid transactionId, 
                TransactionDto transactionDto, 
                CancellationToken ct)
		    {
                var userId = User.GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var cmd = new UpdateTransactionCommand(userId.Value, transactionId, transactionDto);

                var result = await handler.Handle(cmd, ct);

                return result.Match(
                    onSuccess: Ok,
                    onFailure: err => err.Code switch
                    {
                        ErrorTypes.InvalidInput
                            or ErrorTypes.ValidationFailed
                            or ErrorTypes.TypeMismatch
                                => BadRequest(err),

                        ErrorTypes.NotFound => NotFound(err),
                        ErrorTypes.UpdateFailed => StatusCode(500, err),

                        _ => StatusCode(500, "An unexpected error occurred.")
                    });
            }

		    [HttpDelete("{transactionId:guid}")]
            [EnableRateLimiting("write")]
		    public async Task<IActionResult> DeleteTransaction(
                [FromServices] DeleteTransactionHandler handler,
                Guid transactionId,
                CancellationToken ct)  
		    {
                var userId = User.GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var cmd = new DeleteTransactionCommand(userId.Value, transactionId);

                var result = await handler.Handle(cmd, ct);

                return result.Match<IActionResult>(
                    onSuccess: NoContent,
                    onFailure: NotFound);
            }

            [HttpGet("balance")]
            [EnableRateLimiting("sensitive")]
            public async Task<ActionResult<BalanceSummaryResponse>> GetUserBalance(
                [FromServices] GetBalanceHandler handler,
                CancellationToken ct)
            {
                var userId = User.GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var balance = await handler.Handle(new GetBalanceQuery(userId.Value), ct);

                return Ok(balance.Value);
            }
        }
    }
