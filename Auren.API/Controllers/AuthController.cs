using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
		public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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

			var result = await _userRepository.RegisterAsync(request);

			if(result.Success && result.User != null)
			{
				var user = await _userManager.FindByEmailAsync(request.Email);
				if(user != null)
				{
					await _signInManager.SignInAsync(user, isPersistent: true);
					return Ok(result);
				}
            }

			return result.Success ? Ok(result) : BadRequest(result);
        }

		[HttpPost]
		public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
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

			var result = await _userRepository.LoginAsync(request);

			if (result.Success) return Ok(result);

			await Task.Delay(1000);
			return BadRequest(result);
		}
    }
}
