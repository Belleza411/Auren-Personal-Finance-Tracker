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

            return result.Match(
                onSuccess: Ok,
                onFailure: err => err.Code switch
                {
                    ErrorTypes.ValidationFailed => BadRequest(err),
                    ErrorTypes.EmailAlreadyInUse => Conflict(err),
                    ErrorTypes.UploadFailed => StatusCode(500, err),
                    ErrorTypes.CreateFailed => StatusCode(500, err),
                    _ => StatusCode(500, err)
                });
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<ActionResult<AuthResponse>> Login(
            [FromServices] LoginHandler handler,
            LoginRequest request,
            CancellationToken ct)
        {
            var result = await handler.Handle(new LoginCommand(request), ct);

            await Task.Delay(1000, ct);
            return result.Match(
                onSuccess: Ok,
                onFailure: err => err.Code switch
                {
                    ErrorTypes.ValidationFailed => BadRequest(err),
                    ErrorTypes.UserLockedOut => StatusCode(429, err),
                    ErrorTypes.InvalidInput => BadRequest(err),
                    _ => StatusCode(500, err)
                });
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

            return result.Match<ActionResult<AuthResponse>>(
                onSuccess: value => Ok(value),
                onFailure: err => BadRequest(err));
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

            var result = await handler.Handle(
                new DeleteAccountCommand(userId.Value, password), ct);

            return result.Match<IActionResult>(
                 onSuccess: () =>
                 {
                     HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                     return Ok("Account deleted successfully.");
                 },
                 onFailure: err => err.Code switch {
                     ErrorTypes.NotFound => NotFound(result.Error),
                     ErrorTypes.InvalidInput => BadRequest(result.Error),
                     ErrorTypes.DeleteFailed => StatusCode(500, result.Error),
                     _ => StatusCode(500, result.Error)
                 });
        }

        [HttpPost("logout")]
        [EnableRateLimiting("write")]
        public async Task<ActionResult<AuthResponse>> Logout([FromServices] LogoutHandler handler, CancellationToken ct)
        {
            var userId = User.GetCurrentUserId();

            if (userId == null)
                return BadRequest("Logout attempted without valid user session.");

            var result = await handler.Handle(new LogoutCommand(userId.Value), ct);

            return result.Match<ActionResult<AuthResponse>>(
                onSuccess: value => Ok(value),
                onFailure: err => err.Code switch
                {
                    ErrorTypes.LogoutFailed => BadRequest(result.Error),
                    ErrorTypes.InvalidInput => BadRequest(result.Error),
                    _ => StatusCode(500, result.Error)
                });
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
