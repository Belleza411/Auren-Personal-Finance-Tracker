using Auren.Application.Common.Result;
using Auren.Application.DTOs.Filters;
using Auren.Application.DTOs.Requests;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
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
        private readonly IGoalService _goalService;

		public GoalsController(ILogger<GoalsController> logger, IGoalService goalService)
		{
			_logger = logger;
			_goalService = goalService;
		}

		[HttpGet]
        public async Task<ActionResult<IEnumerable<Goal>>> GetAllGoals(
            [FromQuery] GoalFilter goalFilter,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 3,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var goals = await _goalService.GetGoals(userId.Value, goalFilter, pageSize, pageNumber, cancellationToken);
                return Ok(goals.Value);
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
            if (userId == null) return Unauthorized();
            
            try
            {
                var goal = await _goalService.GetGoalById(goalId, userId.Value, cancellationToken);
                return goal.IsSuccess ? Ok(goal.Value) : NotFound($"Goal id of {goalId} not found. ");
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
            if (userId == null) return Unauthorized();
            
            try
            {
                var createdGoal = await _goalService.CreateGoal(goalDto, userId.Value, cancellationToken);

                if(!createdGoal.IsSuccess)
                {
                    return createdGoal.Error.Code switch
                    {
                        ErrorType.InvalidInput
                            or ErrorType.ValidationFailed
                                => BadRequest(createdGoal.Error),

                        ErrorType.CreateFailed => StatusCode(500, createdGoal.Error),

                        _ => StatusCode(500, "An unexpected error occurred.")
                    };
                }

                return CreatedAtAction(nameof(GetGoalById), new { goalId = createdGoal.Value.GoalId }, createdGoal.Value);
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
            if (userId == null) return Unauthorized();
          
            
            try
            {
                var updatedGoal = await _goalService.UpdateGoal(goalId, userId.Value, goalDto, cancellationToken);

                if (!updatedGoal.IsSuccess)
                {
                    return updatedGoal.Error.Code switch
                    {
                        ErrorType.InvalidInput
                            or ErrorType.ValidationFailed
                                => BadRequest(updatedGoal.Error),

                        ErrorType.NotFound => NotFound(updatedGoal.Error),
                        ErrorType.UpdateFailed => StatusCode(500, updatedGoal.Error),

                        _ => StatusCode(500, "An unexpected error occurred.")
                    };
                }

                return Ok(updatedGoal.Value);
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
            if (userId == null) return Unauthorized();
           
            try
            {
                var deleted = await _goalService.DeleteGoal(goalId, userId.Value, cancellationToken);

                return deleted.IsSuccess ? NoContent() : NotFound(deleted.Error);
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
            if (userId == null) return Unauthorized();
            
            try
            {
              
                var result = await _goalService.AddMoneyToGoal(goalId, userId.Value, amount, cancellationToken);

                if(!result.IsSuccess)
                {
                    return result.Error.Code switch
                    {
                        ErrorType.AmountMustBePositive => BadRequest(result.Error),
                        ErrorType.NotEnoughBalance => BadRequest(result.Error),
                        ErrorType.NotFound => NotFound(result.Error),
                        ErrorType.UpdateFailed => StatusCode(500, result.Error),
                        _ => StatusCode(500, "An unexpected error occurred.")
                    };
                }

                return Ok(result.Value);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add moeny to goal {GoalId} for user {UserId}", goalId, userId);
                return StatusCode(500, "An error occurred while updating the goal. Please try again later.");
            }
        }
    }
}
