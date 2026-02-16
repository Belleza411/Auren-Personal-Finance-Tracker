using Auren.Application.Common.Result;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.User;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auren.API.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController(ITokenService tokenService, IUserService userService) : ControllerBase
	{

		[HttpPost("register")]
		public async Task<ActionResult<AuthResponse>> Register([FromForm] RegisterRequest request, CancellationToken cancellationToken)
		{
			var result = await userService.RegisterAsync(request, cancellationToken);

			if(!result.IsSuccess)
			{
				return result.Error.Code switch
				{
					ErrorTypes.ValidationFailed => BadRequest(result.Error),
					ErrorTypes.EmailAlreadyInUse => Conflict(result.Error),
					ErrorTypes.UpdateFailed => StatusCode(500, result.Error),
					ErrorTypes.CreateFailed => StatusCode(500, result.Error),
                    _ => StatusCode(500, result.Error)
				};
            }

			return Ok(result.Value);
        }

		[HttpPost("login")]
		public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
		{
			var result = await userService.LoginAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    ErrorTypes.ValidationFailed => BadRequest(result.Error),
					ErrorTypes.UserLockedOut => StatusCode(429, result.Error),
					ErrorTypes.InvalidInput => BadRequest(result.Error),
                    _ => StatusCode(500, result.Error)
                };
            }

            await Task.Delay(1000, cancellationToken);
			return Ok(result.Value);
		}

		[HttpPost("logout")]
		public async Task<ActionResult<AuthResponse>> Logout(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();

			if (userId == null)			
				return BadRequest("Logout attempted without valid user session");
            
            await tokenService.RevokeAllUserRefreshTokens(userId.Value);

			var logoutResult  = await userService.LogoutAsync(cancellationToken);

			if (!logoutResult.IsSuccess)
			{
				return logoutResult.Error.Code switch
				{
					ErrorTypes.LogoutFailed => BadRequest(logoutResult.Error),
					ErrorTypes.InvalidInput => BadRequest(logoutResult.Error),
					_ => StatusCode(500, logoutResult.Error)
				};
            }

            return Ok(new AuthResponse
			{
				Success = true,
				Message = "Logged out successfully"
			});
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var refreshToken = Request.Cookies["Auren.Session"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var storedToken = await tokenService.GetRefreshToken(userId.Value, refreshToken);

            if (!storedToken.IsSuccess)
                return Unauthorized();

            var newToken = await tokenService.RotateRefreshToken(storedToken.Value);

            if (!newToken.IsSuccess)
                return Unauthorized();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new(ClaimTypes.Email, storedToken.Value.User.Email!)
            };

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
                });

            return Ok();
        }
    }
}
