using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.Extensions;
using Auren.Application.Interfaces.Repositories;
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
		private readonly IUserRepository _userRepository;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ILogger<AuthController> _logger;

		public AuthController(ITokenRepository tokenRepository,
			IUserRepository userRepository,
			UserManager<ApplicationUser> userManager,
			ILogger<AuthController> logger)
		{
			_tokenRepository = tokenRepository;
			_userRepository = userRepository;
			_userManager = userManager;
			_logger = logger;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromForm] RegisterRequest request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.RegisterAsync(request, cancellationToken);

			if(result.Success && result.User != null)
			{
				var user = await _userManager.FindByEmailAsync(request.Email);
				if(user != null)
				{
					var accessToken = _tokenRepository.GenerateAccessTokenAsync(user);
					var refreshToken = await _tokenRepository.GenerateRefreshTokenAsync(user);

					var claims = new List<Claim>
					{
						new(ClaimTypes.Email, user.Email!),
						new(ClaimTypes.NameIdentifier, user.Id.ToString()),
						new("UserId", user.UserId.ToString()),
						new("AccessToken", accessToken),
						new("RefreshToken", refreshToken.Token)
					};

					await SignInUserAsync(claims);

                    _logger.LogInformation("User {Email} logged in successfully", user.Email);
                    return Ok(result);
				}
            }

			return result.Success ? Ok(result) : BadRequest(result);
        }

		[HttpPost("login")]
		public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
		{
			var result = await _userRepository.LoginAsync(request, cancellationToken);

			if(result.Success && result.User != null)
			{
				var user = await _userManager.FindByEmailAsync(request.Email);
				if(user != null)
				{
					var accessToken = _tokenRepository.GenerateAccessTokenAsync(user);
					var refreshToken = await _tokenRepository.GenerateRefreshTokenAsync(user);
					
					var claims = new List<Claim>
					{
						new(ClaimTypes.Email, user.Email!),
						new(ClaimTypes.NameIdentifier, user.Id.ToString()),
						new("UserId", user.UserId.ToString()),
						new("AccessToken", accessToken),
						new("RefreshToken", refreshToken.Token)
					};
					
					await SignInUserAsync(claims);
					_logger.LogInformation("User {Email} logged in successfully", user.Email);
					return Ok(result);
                }
            }

			await Task.Delay(1000, cancellationToken);
			return BadRequest(result);
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			try
			{
				var userId = User.GetCurrentUserId();

				if (userId == null)
				{
					_logger.LogWarning("Logout attempted without a valid user session");
					return BadRequest("Logout attempted without valid user session");
                }

                await _tokenRepository.RevokeAllUserRefreshTokensAsync(userId.Value);

				await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

				_logger.LogInformation("User logged out successfully");

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

        private async Task SignInUserAsync(List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                IssuedUtc = DateTimeOffset.UtcNow
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
