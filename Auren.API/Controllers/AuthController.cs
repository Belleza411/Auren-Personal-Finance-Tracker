using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
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
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ILogger<AuthController> _logger;

		public AuthController(ITokenRepository tokenRepository,
			IUserRepository userRepository,
            SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			ILogger<AuthController> logger)
		{
			_tokenRepository = tokenRepository;
			_userRepository = userRepository;
            _signInManager = signInManager;
			_userManager = userManager;
			_logger = logger;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromForm] RegisterRequest request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new AuthResponse
				{
					Success = false,
					Message = "Invalid Input",
                    Errors = ModelState.SelectMany(x => x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>()).ToList()
                });
			}

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
						new Claim(ClaimTypes.Email, user.Email!),
						new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
						new Claim("UserId", user.UserId.ToString()),
						new Claim("AccessToken", accessToken),
						new Claim("RefreshToken", refreshToken.Token)
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
			if(!ModelState.IsValid)
			{
				return BadRequest(new AuthResponse
				{
					Success = false,
					Message = "Invalid input",
                    Errors = ModelState.SelectMany(x => x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>()).ToList()
                });
			}

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
						new Claim(ClaimTypes.Email, user.Email!),
						new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
						new Claim("UserId", user.UserId.ToString()),
						new Claim("AccessToken", accessToken),
						new Claim("RefreshToken", refreshToken.Token)
					};
					
					await SignInUserAsync(claims);
					_logger.LogInformation("User {Email} logged in successfully", user.Email);
					return Ok(result);
                }
            }

			await Task.Delay(1000);
			return BadRequest(result);
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			try
			{
				var userIdClaim = User.FindFirst("UserId")?.Value;
				if(!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
				{
					await _tokenRepository.RevokeAllUserRefreshTokensAsync(userId);
                    _logger.LogInformation("Revoked refresh tokens for user {UserId}", userId);
                }

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
