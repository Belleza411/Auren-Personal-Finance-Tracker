using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Auth.Commands.Login;
using Auren.Application.Features.Auth.Commands.Logout;
using Auren.Application.Features.Auth.Commands.Register;
using Auren.Application.Features.Auth.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Auren.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(
    ITokenService tokenService,
    RegisterHandler registerHandler,
    LoginHandler loginHandler,
    LogoutHandler logoutHandler) : ControllerBase
    {
        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Register(
            [FromForm] RegisterRequest request,
            CancellationToken ct)
        {
            var result = await registerHandler.Handle(new RegisterCommand(request), ct);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    ErrorTypes.ValidationFailed => BadRequest(result.Error),
                    ErrorTypes.EmailAlreadyInUse => Conflict(result.Error),
                    ErrorTypes.UploadFailed => StatusCode(500, result.Error),
                    ErrorTypes.CreateFailed => StatusCode(500, result.Error),
                    _ => StatusCode(500, result.Error)
                };
            }

            return Ok(result.Value);
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Login(
            LoginRequest request,
            CancellationToken ct)
        {
            var result = await loginHandler.Handle(new LoginCommand(request), ct);

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

            await Task.Delay(1000, ct);
            return Ok(result.Value);
        }

        [HttpPost("logout")]
        [EnableRateLimiting("write")]
        public async Task<ActionResult<AuthResponse>> Logout(CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
                return BadRequest("Logout attempted without valid user session.");

            var result = await logoutHandler.Handle(new LogoutCommand(userId.Value), ct);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    ErrorTypes.LogoutFailed => BadRequest(result.Error),
                    ErrorTypes.InvalidInput => BadRequest(result.Error),
                    _ => StatusCode(500, result.Error)
                };
            }

            return Ok(new AuthResponse { Success = true, Message = "Logged out successfully." });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var refreshToken = Request.Cookies["Auren.Session"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();

            var storedToken = await tokenService.GetRefreshToken(userId.Value, refreshToken, ct);
            if (!storedToken.IsSuccess)
                return Unauthorized();

            var newToken = await tokenService.RotateRefreshToken(storedToken.Value, ct);
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
