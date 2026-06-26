using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Auth.Commands.ChangePassword;
using Auren.Application.Features.Auth.Commands.DeleteAccount;
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
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Register(
            [FromServices] RegisterHandler handler,
            [FromForm] RegisterRequest request,
            CancellationToken ct)
        {
            var result = await handler.Handle(new RegisterCommand(request), ct);

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
            [FromServices] LoginHandler handler,
            LoginRequest request,
            CancellationToken ct)
        {
            var result = await handler.Handle(new LoginCommand(request), ct);

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

        [HttpPut("change-password")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> ChangePassword(
            [FromServices] ChangePasswordHandler handler,
            ChangePasswordRequest request,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var cmd = new ChangePasswordCommand(
                userId.Value,
                new ChangePasswordDto(
                    request.CurrentPassword,
                    request.NewPassword,
                    request.ConfirmPassword)
                );

            var result = await handler.Handle(cmd, ct);

            return result.IsSuccess
                ? Ok("Password changed successfully.")
                : BadRequest(result.Error);
        }

        [HttpDelete("delete-account")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> DeleteAccount(
            [FromServices] DeleteAccountHandler handler,
            string password,
            CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (string.IsNullOrEmpty(password))
                return BadRequest("Password is empty");

            var result = await handler.Handle(
                new DeleteAccountCommand(userId.Value, password), ct);

            if (!result.IsSuccess)
            {
                return result.Error.Code switch
                {
                    ErrorTypes.NotFound => NotFound(result.Error),
                    ErrorTypes.InvalidInput => BadRequest(result.Error),
                    ErrorTypes.DeleteFailed => StatusCode(500, result.Error),
                    _ => StatusCode(500, result.Error)
                };
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Account deleted successfully.");
        }

        [HttpPost("logout")]
        [EnableRateLimiting("write")]
        public async Task<ActionResult<AuthResponse>> Logout([FromServices] LogoutHandler handler, CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
                return BadRequest("Logout attempted without valid user session.");

            var result = await handler.Handle(new LogoutCommand(userId.Value), ct);

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
        public async Task<IActionResult> Refresh([FromServices] ITokenService tokenService, CancellationToken ct)
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
