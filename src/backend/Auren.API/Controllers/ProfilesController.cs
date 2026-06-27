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
    public class ProfilesController : ControllerBase
	{
		[HttpGet("me")]
        [EnableRateLimiting("read")]

        public async Task<ActionResult<UserResponse>> GetUserProfile([FromServices] GetUserProfileHandler handler, CancellationToken ct)
		{
			var userId = User.GetCurrentUserId();

			if (userId == null) return Unauthorized();

			var result = await handler.Handle(new GetUserProfileQuery(userId.Value), ct);

			return result.Match<ActionResult<UserResponse>>(
				onSuccess: value => Ok(value),
				onFailure: err => NotFound(err));
        }

		[HttpPut("update-user")]
        [EnableRateLimiting("write")]

        public async Task<ActionResult<UserResponse>> UpdateUserProfile(
			[FromServices] UpdateProfileHandler handler,
			[FromForm] UserDto userDto,
			CancellationToken ct)
		{
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

			var cmd = new UpdateProfileCommand(userId.Value, userDto);

			var result = await handler.Handle(cmd, ct);

			return result.Match(
				onSuccess: value => Ok(value),
				onFailure: err => err.Code switch
				{
					ErrorTypes.ValidationFailed => BadRequest(err),
					ErrorTypes.NotFound => NotFound(err),
					ErrorTypes.UpdateFailed => StatusCode(500, err),
					_ => StatusCode(500, err)
				});
        }
    }
}
