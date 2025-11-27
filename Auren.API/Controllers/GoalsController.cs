using Auren.API.DTOs.Filters;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Extensions;
using Auren.API.Helpers.Result;
using Auren.API.Models.Domain;
using Auren.API.Models.Enums;
using Auren.API.Repositories.Interfaces;
using Auren.API.Services.Interfaces;
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
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;

		public GoalsController(ILogger<GoalsController> logger, IGoalRepository goalRepository, ITransactionRepository transactionRepository, ICategoryRepository categoryRepository, ITransactionService transactionService, ICategoryService categoryService)
		{
			_logger = logger;
			_goalRepository = goalRepository;
			_transactionRepository = transactionRepository;
			_categoryRepository = categoryRepository;
			_transactionService = transactionService;
			_categoryService = categoryService;
		}

		[HttpGet]
        public async Task<ActionResult<IEnumerable<Goal>>> GetAllGoals(CancellationToken cancellationToken,
            [FromQuery] GoalFilter goalFilter,
            [FromQuery] int? pageNumber = 1, 
            [FromQuery] int? pageSize = 3)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var goals = await _goalRepository.GetGoalsAsync(userId.Value, cancellationToken, goalFilter, pageSize, pageNumber);
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
            var userId = User.GetCurrentUserId();
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
            var userId = User.GetCurrentUserId();
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
            var userId = User.GetCurrentUserId();
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
            var userId = User.GetCurrentUserId();
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
            var userId = User.GetCurrentUserId();
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

                var currentBalance = await _transactionRepository.GetBalanceAsync(userId.Value, cancellationToken, true);

                if (currentBalance < amount)
                {
                    return BadRequest("Insufficient Balance");
                }

                var goalCategory = new CategoryDto("Goal Transfer", TransactionType.Expense);

                var existingCategory = await _categoryService.GetCategoryByName(userId.Value, goalCategory, cancellationToken);

                if (existingCategory == null)
                {
                    var createdCategoryResult = await _categoryService.CreateCategory(goalCategory, userId.Value, cancellationToken);
                    if (!createdCategoryResult.IsSuccess || createdCategoryResult.Value == null)
                    {
                        return StatusCode(500, "Failed to create category for goal transfer.");
                    }
                    existingCategory = Result.Success<Category?>(createdCategoryResult.Value);
                }

                if (existingCategory?.Value == null)
                {
                    return StatusCode(500, "Category for goal transfer is missing.");
                }

                var transaction = await _transactionService.CreateTransaction(new TransactionDto(
                    $"Transfer to goal: {goal.Name}",
                    amount,
                    existingCategory.Value.Name,
                    existingCategory.Value.TransactionType,
                    PaymentType.Other
                ), userId.Value, cancellationToken);

                var updatedGoal = await _goalRepository.AddMoneyToGoalAsync(goalId, userId.Value, amount, cancellationToken);

                if(updatedGoal == null)
                {
                    _logger.LogError("Transaction created but failed to update goal {GoalId} for user {UserId}. Manual reconciliation needed.", goalId, userId);
                    return StatusCode(500, "Transaction created but goal update failed. ");
                }

                var newBalance = await _transactionRepository.GetBalanceAsync(userId.Value, cancellationToken, true);

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
    }
}
