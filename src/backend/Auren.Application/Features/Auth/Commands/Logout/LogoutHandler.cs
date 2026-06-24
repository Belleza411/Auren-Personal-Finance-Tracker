using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Auren.Application.Features.Auth.Commands.Logout
{
    public class LogoutHandler( 
        ITokenService tokenService,
        IHttpContextAccessor http)
    {
        public async Task<Result<bool>> Handle(LogoutCommand cmd, CancellationToken ct)
        {
            await tokenService.RevokeAllUserRefreshTokens(cmd.UserId, ct);

            await http.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Result.Success(true);
        }
    }
}
