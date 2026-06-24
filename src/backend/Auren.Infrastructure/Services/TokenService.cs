using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Auren.Infrastructure.Services
{
    public class TokenService(IAuthDbContext db,
        UserManager<ApplicationUser> userManager) : ITokenService
    {
        public async Task<Result<RefreshToken>> GetRefreshToken(Guid userId, string token, CancellationToken ct)
        {
            var refreshToken = await db.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.UserId == userId &&
                    rt.Token == token, ct);

            return refreshToken == null
                ? Result.Failure<RefreshToken>(Error.NotFound("Refresh token not found."))
                : Result.Success(refreshToken);
        }

        public async Task<Result<RefreshToken>> GenerateRefreshToken(Guid userId, CancellationToken ct)
        {
            var refreshToken = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = userId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await db.RefreshTokens.AddAsync(refreshToken, ct);
            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(refreshToken)
                : Result.Failure<RefreshToken>(Error.CreateFailed("Failed to generate refresh token."));
        }

        public async Task<Result<RefreshToken>> RotateRefreshToken(RefreshToken oldToken, CancellationToken ct)
        {
            oldToken.IsRevoked = true;
            oldToken.ReasonRevoked = "Rotated";

            var newToken = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = oldToken.UserId,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                ReplacedByToken = oldToken.Token
            };

            await db.RefreshTokens.AddAsync(newToken, ct);
            var saved = await db.SaveChangesAsync(ct) > 0;

            return saved
                ? Result.Success(newToken)
                : Result.Failure<RefreshToken>(Error.CreateFailed("Failed to rotate refresh token."));
        }

        public async Task<Result> RevokeAllUserRefreshTokens(Guid userId, CancellationToken ct)
        {
            var tokens = await db.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync(ct);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.ReasonRevoked = "Revoked by user logout";
            }

            await db.SaveChangesAsync(ct);
            return Result.Success();
        }

        public async Task<bool> ValidateRefreshTokenAsync(CookieValidatePrincipalContext context)
        {
            try
            {
                var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.Principal?.FindFirst("UserId")?.Value;

                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return true;

                if (string.IsNullOrEmpty(email))
                    return true;

                var user = await userManager.FindByEmailAsync(email);
                if (user == null || user.UserId != userId)
                {
                    context.RejectPrincipal();
                    return false;
                }

                var hasValidToken = await db.RefreshTokens
                    .AnyAsync(rt => rt.UserId == userId && rt.IsActive);

                if (!hasValidToken && context.Properties.ExpiresUtc.HasValue)
                {
                    context.RejectPrincipal();
                    return false;
                }

                var expiresUtc = context.Properties.ExpiresUtc;
                var shouldRenew = expiresUtc.HasValue && DateTime.UtcNow > expiresUtc.Value;

                if (shouldRenew)
                {
                    var hasActiveToken = await db.RefreshTokens
                        .AnyAsync(rt =>
                            rt.UserId == userId &&
                            rt.IsActive &&
                            rt.ExpiryDate > DateTime.UtcNow);

                    if (!hasActiveToken)
                    {
                        context.RejectPrincipal();
                        return false;
                    }

                    context.Properties.IssuedUtc = DateTimeOffset.UtcNow;
                    context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10);
                    context.ShouldRenew = true;
                }

                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
