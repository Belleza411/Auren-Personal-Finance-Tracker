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
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Auren.API.Controllers
{
	[Route("api/goals")]
	[ApiController]
    [EnableRateLimiting("fixed")]
    public class GoalsController(IGoalService goalService) : ControllerBase
	{
		[HttpGet]
        public async Task<ActionResult<IEnumerable<Goal>>> GetAllGoals(
            [FromQuery] GoalFilter goalFilter,
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 3,
            CancellationToken cancellationToken = default)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
  
            var goals = await goalService.GetGoals(userId.Value, goalFilter, pageSize, pageNumber, cancellationToken);
            return Ok(goals.Value);
        }

        [HttpGet("{goalId:guid}")]
        public async Task<ActionResult<Goal>> GetGoalById([FromRoute] Guid goalId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            
            var goal = await goalService.GetGoalById(goalId, userId.Value, cancellationToken);
            return goal.IsSuccess ? Ok(goal.Value) : NotFound($"Goal id of {goalId} not found. ");
            
        }

        [HttpPost]
        public async Task<ActionResult<Goal>> CreateGoal([FromBody] GoalDto goalDto, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
             
            var createdGoal = await goalService.CreateGoal(goalDto, userId.Value, cancellationToken);

            if(!createdGoal.IsSuccess)
            {
                return createdGoal.Error.Code switch
                {
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                            => BadRequest(createdGoal.Error),

                    ErrorTypes.CreateFailed => StatusCode(500, createdGoal.Error),

                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }

            return CreatedAtAction(nameof(GetGoalById), new { goalId = createdGoal.Value.GoalId }, createdGoal.Value);
        }

        [HttpPut("{goalId:guid}")]
        public async Task<ActionResult<Goal>> UpdateGoal([FromRoute] Guid goalId, [FromBody] GoalDto goalDto, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
          
            var updatedGoal = await goalService.UpdateGoal(goalId, userId.Value, goalDto, cancellationToken);

            if (!updatedGoal.IsSuccess)
            {
                return updatedGoal.Error.Code switch
                {
                    ErrorTypes.InvalidInput
                        or ErrorTypes.ValidationFailed
                            => BadRequest(updatedGoal.Error),

                    ErrorTypes.NotFound => NotFound(updatedGoal.Error),
                    ErrorTypes.UpdateFailed => StatusCode(500, updatedGoal.Error),

                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }

            return Ok(updatedGoal.Value);          
        }

        [HttpDelete("{goalId:guid}")]
        public async Task<IActionResult> DeleteGoal([FromRoute] Guid goalId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
           
            var deleted = await goalService.DeleteGoal(goalId, userId.Value, cancellationToken);

            return deleted.IsSuccess ? NoContent() : NotFound(deleted.Error);
           
        }

        [HttpPut("{goalId:guid}/add-money")]
        public async Task<IActionResult> AddMoneyToGoal([FromBody] decimal amount, [FromRoute] Guid goalId, CancellationToken cancellationToken)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();
 
            var result = await goalService.AddMoneyToGoal(goalId, userId.Value, amount, cancellationToken);

            if(!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    ErrorTypes.AmountMustBePositive => BadRequest(result.Error),
                    ErrorTypes.NotEnoughBalance => BadRequest(result.Error),
                    ErrorTypes.NotFound => NotFound(result.Error),
                    ErrorTypes.UpdateFailed => StatusCode(500, result.Error),
                    _ => StatusCode(500, "An unexpected error occurred.")
                };
            }

            return Ok(result.Value);
        }
    }
}
