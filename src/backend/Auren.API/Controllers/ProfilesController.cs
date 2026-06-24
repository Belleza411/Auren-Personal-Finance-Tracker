using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Auth.DTOs;
using Auren.Application.Features.Profile.Commands.UpdateProfile;
using Auren.Application.Features.Profile.Queries.GetUserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
	
namespace Auren.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
    [EnableRateLimiting("fixed")]
    public class ProfilesController(
		GetUserProfileHandler getHanlder,
		UpdateProfileHandler updateHandler) : ControllerBase
	{
		[HttpGet("me")]
        [EnableRateLimiting("read")]

        public async Task<ActionResult<UserResponse>> GetUserProfile(CancellationToken ct)
		{
			var userId = User.GetCurrentUserId();

			if (userId == null) return Unauthorized();

			var userProfile = await getHanlder.Handle(new GetUserProfileQuery(userId.Value), ct);

			return userProfile.IsSuccess ? Ok(userProfile.Value) : NotFound(userProfile.Error);
        }

		[HttpPut("update-user")]
        [EnableRateLimiting("write")]

        public async Task<ActionResult<UserResponse>> UpdateUserProfile([FromForm] UserDto userDto, CancellationToken ct)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

			var cmd = new UpdateProfileCommand(userId.Value, userDto);

			var updateProfile = await updateHandler.Handle(cmd, ct);

 			if(!updateProfile.IsSuccess)
			{
				return updateProfile.Error.Code switch
				{
					ErrorTypes.ValidationFailed => BadRequest(updateProfile.Error),
					ErrorTypes.NotFound => NotFound(updateProfile.Error),
					ErrorTypes.UpdateFailed => StatusCode(500, updateProfile.Error),
					_ => StatusCode(500, updateProfile.Error)
				};
			}

			return Ok(updateProfile.Value);
        }
    }
}
