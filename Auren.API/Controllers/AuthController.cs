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
	public class AuthController : ControllerBase
	{
		private readonly ITokenRepository _tokenRepository;
		private readonly ILogger<AuthController> _logger;
		private readonly IUserService _userService;

		public AuthController(ITokenRepository tokenRepository,
			ILogger<AuthController> logger,
			IUserService userService)
		{
			_tokenRepository = tokenRepository;
			_logger = logger;
			_userService = userService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromForm] RegisterRequest request, CancellationToken cancellationToken)
		{
			var result = await _userService.RegisterAsync(request, cancellationToken);

			if(!result.IsSuccess)
			{
				return result.Error.Code switch
				{
					ErrorType.ValidationFailed => BadRequest(result.Error),
					ErrorType.EmailAlreadyInUse => Conflict(result.Error),
					ErrorType.UpdateFailed => StatusCode(500, result.Error),
					ErrorType.CreateFailed => StatusCode(500, result.Error),
                    _ => StatusCode(500, result.Error)
				};
            }

			return Ok(result.Value);
        }

		[HttpPost("login")]
		public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
		{
			var result = await _userService.LoginAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    ErrorType.ValidationFailed => BadRequest(result.Error),
					ErrorType.UserLockedOut => StatusCode(429, result.Error),
					ErrorType.InvalidInput => BadRequest(result.Error),
                    _ => StatusCode(500, result.Error)
                };
            }

            await Task.Delay(1000, cancellationToken);
			return Ok(result.Value);
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout(CancellationToken cancellationToken)
		{
			var userId = User.GetCurrentUserId();

			if (userId == null)
			{
				_logger.LogWarning("Logout attempted without a valid user session");
				return BadRequest("Logout attempted without valid user session");
            }

			try
			{

                await _tokenRepository.RevokeAllUserRefreshTokensAsync(userId.Value);

				var logoutResult  = await _userService.LogoutAsync(cancellationToken);

				if (!logoutResult.IsSuccess)
				{
					return logoutResult.Error.Code switch
					{
						ErrorType.LogoutFailed => BadRequest(logoutResult.Error),
						ErrorType.InvalidInput => BadRequest(logoutResult.Error),
						_ => StatusCode(500, logoutResult.Error)
					};
                }

                return Ok(new
				{
					Success = true,
					Message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(new { Success = false, Message = "Logout failed" });
            }
        }
    }
}
