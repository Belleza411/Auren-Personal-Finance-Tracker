using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Extensions;
using Auren.API.Repositories.Interfaces;
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
		private readonly IProfileRepository _profileRepository;
        private readonly ITransactionRepository _transactionRepository;

		public ProfilesController(ILogger<ProfilesController> logger,
			IProfileRepository profileRepository,
			ITransactionRepository transactionRepository)
		{
			_logger = logger;
			_profileRepository = profileRepository;
			_transactionRepository = transactionRepository;
		}

		[HttpGet("me")]
		public async Task<ActionResult<UserResponse>> GetUserProfile(CancellationToken cancellationToken)
		{
			try
			{
				var userId = User.GetCurrentUserId();

				if (userId == null) return Unauthorized();

				var userProfile = await _profileRepository.GetUserProfile(userId.Value, cancellationToken);

				if(userProfile == null) return NotFound("User profile not found.");

				return Ok(userProfile);
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user profile");
				return StatusCode(500, "An error occurred while retrieving the profile.");
            }
        }

		[HttpPut("update-user")]
		public async Task<ActionResult<UserResponse>> UpdateUserProfile([FromBody] UserDto userDto, CancellationToken cancellationToken)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
			{
				var updateProfile = await _profileRepository.UpdateUserProfile(userId.Value, userDto, cancellationToken);

				if (updateProfile == null) return NotFound("User profile not found.");
            
				return Ok(updateProfile);
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
				var balance = await _transactionRepository.GetBalanceAsync(userId.Value, cancellationToken, true);

				return Ok(balance);
            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving user balance");
				return StatusCode(500, "An error occurred while retrieving the balance.");
            }
        }
    }
}
