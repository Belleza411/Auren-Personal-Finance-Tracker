using Auren.Application.Extensions;
using Auren.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auren.API.Controllers
{
    [Route("api/token")]
    [ApiController]
    public class TokensController(ITokenService tokenService) : ControllerBase
    {
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
