using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Extensions;
using Auren.Application.Features.Auth.DTOs;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Auren.Application.Features.Auth.Helper
{
    public static class AuthHelper
    {
        public static async Task<Result<AuthResponse>> SignInAsync(
            ApplicationUser user,
            string message,
            ITokenService tokenService,
            IHttpContextAccessor http,
            CancellationToken ct)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new("UserId", user.UserId.ToString()),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await tokenService.GenerateRefreshToken(user.UserId, ct);

            await http.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    IssuedUtc = DateTimeOffset.UtcNow,
                    AllowRefresh = true
                });

            return Result.Success(new AuthResponse(user.ToUserResponse(), message));
        }
    }
}
