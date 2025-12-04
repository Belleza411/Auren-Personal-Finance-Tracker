using Auren.Application.Common.Result;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.User;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Auren.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    [EnableRateLimiting("fixed")]
    public class ProfilesController(IProfileService profileService, ITransactionService transactionService) : ControllerBase
	{
		[HttpGet("me")]
		public async Task<ActionResult<UserResponse>> GetUserProfile(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();

			if (userId == null) return Unauthorized();

			var userProfile = await profileService.GetUserProfile(userId.Value, cancellationToken);

			return userProfile.IsSuccess ? Ok(userProfile.Value) : NotFound(userProfile.Error);
        }

		[HttpPut("update-user")]
		public async Task<ActionResult<UserResponse>> UpdateUserProfile([FromForm] UserDto userDto, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

			var updateProfile = await profileService.UpdateUserProfile(userId.Value, userDto, cancellationToken);

             
			if(!updateProfile.IsSuccess)
			{
				return updateProfile.Error.Code switch
				{
					ErrorType.ValidationFailed => BadRequest(updateProfile.Error),
					ErrorType.NotFound => NotFound(updateProfile.Error),
					ErrorType.UpdateFailed => StatusCode(500, updateProfile.Error),
					_ => StatusCode(500, updateProfile.Error)
				};
			}

			return Ok(updateProfile.Value);
        }

		[HttpGet("user-balance")]
		public async Task<ActionResult<decimal>> GetUserBalance(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var balance = await transactionService.GetBalance(userId.Value, BalancePeriod.AllTime, cancellationToken);

			return Ok(balance.Value);
        }
    }
}
