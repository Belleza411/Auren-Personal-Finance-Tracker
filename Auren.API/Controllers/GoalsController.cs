using Auren.API.DTOs.Requests;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Interfaces;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auren.API.Controllers
{
	[Route("api/goals")]
	[ApiController]
	public class GoalsController : ControllerBase
	{
		private readonly ILogger<GoalsController> _logger;
		private readonly IGoalRepository _goalRepository;
        private readonly ITransactionRepository _transactionRepository;

		public GoalsController(ILogger<GoalsController> logger, IGoalRepository goalRepository, ITransactionRepository transactionRepository)
		{
			_logger = logger;
			_goalRepository = goalRepository;
			_transactionRepository = transactionRepository;
		}

		[HttpGet]
        public async Task<ActionResult<IEnumerable<Goal>>> GetAllGoals(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var goals = await _goalRepository.GetGoalsAsync(userId.Value, cancellationToken);
                return Ok(goals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve goals for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving goals. Please try again later.");
            }
        }

        [HttpGet("{goalId:guid}")]
        public async Task<ActionResult<Goal>> GetGoalById([FromRoute] Guid goalId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            try
            {
                var goal = await _goalRepository.GetGoalByIdAsync(goalId, userId.Value, cancellationToken);
                if (goal == null)
                {
                    return NotFound($"Goal id of {goalId} not found. ");
                }
                return Ok(goal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve goal {GoalId} for user {UserId}", goalId, userId);
                return StatusCode(500, "An error occurred while retrieving the goal. Please try again later.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Goal>> CreateGoal([FromBody] GoalDto goalDto, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdGoal = await _goalRepository.CreateGoalAsync(goalDto, userId.Value, cancellationToken);

                return CreatedAtAction(nameof(GetGoalById), new { goalId = createdGoal.GoalId }, createdGoal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create goal for user {UserId}", userId);
                return StatusCode(500, "An error occurred while creating the goal. Please try again later.");
            }
        }

        [HttpPut("{goalId:guid}")]
        public async Task<ActionResult<Goal>> UpdateGoal([FromRoute] Guid goalId, [FromBody] GoalDto goalDto, CancellationToken cancellationToken)
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
                var updatedGoal = await _goalRepository.UpdateGoalAsync(goalId, userId.Value, goalDto, cancellationToken);
                if (updatedGoal == null)
                {
                    return NotFound($"Goal id of {goalId} not found. ");
                }
                return Ok(updatedGoal);
            }
            catch(ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid goal data provided for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update goal {GoalId} for user {UserId}", goalId, userId);
                return StatusCode(500, "An error occurred while updating the goal. Please try again later.");
            }
        }

        [HttpDelete("{goalId:guid}")]
        public async Task<IActionResult> DeleteGoal([FromRoute] Guid goalId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            try
            {
                var deleted = await _goalRepository.DeleteGoalAsync(goalId, userId.Value, cancellationToken);
                if (!deleted)
                {
                    return NotFound($"Goal id of {goalId} not found. ");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete goal {GoalId} for user {UserId}", goalId, userId);
                return StatusCode(500, "An error occurred while deleting the goal. Please try again later.");
            }
        }

        [HttpPut("{goalId:guid}/add-money")]
        public async Task<IActionResult> AddMoneyToGoal([FromBody] decimal amount, [FromRoute] Guid goalId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if(amount < 0)
            {
                return BadRequest("Amount to add must be greater than 0");
            }

            try
            {
                var goal = await _goalRepository.GetGoalByIdAsync(goalId, userId.Value, cancellationToken);
                
                if(goal == null)
                {
                    return NotFound($"Goal id of {goalId} not found. ");
                }

                var currentBalance = await _transactionRepository.GetBalanceAsync(userId.Value, cancellationToken);

                if (currentBalance < amount)
                {
                    return BadRequest("Insufficient Balance");
                }

                var category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    UserId = userId.Value,
                    Name = "Goal Transfer",
                    TransactionType = TransactionType.Expense,
                    CreatedAt = DateTime.UtcNow,
                };

                var transaction = await _transactionRepository.CreateTransactionAsync(new TransactionDto(
                    $"Transfer to goal: {goal.Name}",
                    amount,
                    category.Name,
                    TransactionType.Expense,
                    PaymentType.Other
                ), userId.Value, cancellationToken);


                var updatedGoal = await _goalRepository.AddMoneyToGoalAsync(goalId, userId.Value, amount, cancellationToken);

                if(updatedGoal == null)
                {
                    _logger.LogError("Transaction created but failed to update goal {GoalId} for user {UserId}. Manual reconciliation needed.", goalId, userId);
                    return StatusCode(500, "Transaction created but goal update failed. ");
                }

                var newBalance = await _transactionRepository.GetBalanceAsync(userId.Value, cancellationToken);

                _logger.LogInformation("Successfully added {Amount:C} to goal '{GoalName}' ({GoalId}) for user {UserId}. Balance: {OldBalance:C} -> {NewBalance:C}",
                    amount, updatedGoal.Name, goalId, userId, currentBalance, newBalance);

                return Ok(new
                {
                    Goal = updatedGoal,
                    Transaction = transaction,
                    Balance = new
                    {
                        Previous = currentBalance,
                        Current = newBalance,
                        Difference = newBalance - currentBalance
                    }
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add moeny to goal {GoalId} for user {UserId}", goalId, userId);
                return StatusCode(500, "An error occurred while updating the goal. Please try again later.");
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
