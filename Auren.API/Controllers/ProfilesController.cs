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

namespace Auren.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProfilesController : ControllerBase
	{
		private readonly ILogger<ProfilesController> _logger;
		private readonly IProfileService _profileService;
		private readonly ITransactionService _transactionService;

		public ProfilesController(ILogger<ProfilesController> logger, IProfileService profileService, ITransactionService transactionService)
		{
			_logger = logger;
			_profileService = profileService;
			_transactionService = transactionService;
		}

		[HttpGet("me")]
		public async Task<ActionResult<UserResponse>> GetUserProfile(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();

			if (userId == null) return Unauthorized();

			try
			{

				var userProfile = await _profileService.GetUserProfile(userId.Value, cancellationToken);

				return userProfile.IsSuccess ? Ok(userProfile.Value) : NotFound(userProfile.Error);
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user profile");
				return StatusCode(500, "An error occurred while retrieving the profile.");
            }
        }

		[HttpPut("update-user")]
		public async Task<ActionResult<UserResponse>> UpdateUserProfile([FromForm] UserDto userDto, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
			{
				var updateProfile = await _profileService.UpdateUserProfile(userId.Value, userDto, cancellationToken);

                
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating user profile");
				return StatusCode(500, "An error occurred while updating the profile.");
            }
        }

		[HttpGet("user-balance")]
		public async Task<ActionResult<decimal>> GetUserBalance(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();
			if (userId == null) return Unauthorized();

			try
			{
				var balance = await _transactionService.GetBalance(userId.Value, BalancePeriod.AllTime, cancellationToken);

				return Ok(balance.Value);
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user balance");
				return StatusCode(500, "An error occurred while retrieving the balance.");
            }
        }
    }
}
