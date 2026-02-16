using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Auren.Infrastructure.Repositories
{
	public class TokenRepository(
		AurenAuthDbContext dbContext,
		UserManager<ApplicationUser> userManager) 
		: ITokenRepository
	{
        public async Task<RefreshToken?> GetRefreshTokenAsync(Guid userId, string token)
        {
            return await dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.Token == token && rt.IsActive)
                .FirstOrDefaultAsync();
        }

		public async Task CleanupExpiredTokensAsync()
		{
			var expiredTokens = await dbContext.RefreshTokens
				.Where(rt => rt.ExpiryDate < DateTime.UtcNow)
				.ToListAsync();

			dbContext.RefreshTokens.RemoveRange(expiredTokens);
			await dbContext.SaveChangesAsync();
        }

		public string GenerateAccessTokenAsync()
		{
			var tokenBytes = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(tokenBytes);
			var token = Convert.ToBase64String(tokenBytes);
			return token;
        }

		public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
		{
			await RevokeAllUserRefreshTokensAsync(userId);

            var tokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes);

			var refreshToken = new RefreshToken
			{
				Token = token,
				ExpiryDate = DateTime.UtcNow.AddDays(14),
				UserId = userId
            };

			await dbContext.RefreshTokens.AddAsync(refreshToken);
			await dbContext.SaveChangesAsync();

            return refreshToken;
        }

		public async Task RevokeAllUserRefreshTokensAsync(Guid userId)
		{
			var refreshTokens = await dbContext.RefreshTokens
				.Where(rt => rt.UserId == userId && rt.IsActive)
				.ToListAsync();

			foreach (var token in refreshTokens)
			{
				token.IsRevoked = true;
				token.RevokedAt = DateTime.UtcNow;
				token.ReasonRevoked = "User logged out";
            }

			if(refreshTokens.Count != 0) 
				await dbContext.SaveChangesAsync();
        }

		public async Task RevokeRefreshTokenAsync(string token, string reason)
		{
			var refreshToken = await dbContext.RefreshTokens
				.FirstOrDefaultAsync(rt => rt.Token == token);

			if(refreshToken != null && refreshToken.IsActive)
			{
				refreshToken.IsRevoked = true;
				refreshToken.RevokedAt = DateTime.UtcNow;
				refreshToken.ReasonRevoked = reason;
            }

			await dbContext.SaveChangesAsync();
		}

		public async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken token)
		{
            await RevokeRefreshTokenAsync(token.Token, "Rotated");

            var newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshToken = new RefreshToken
            {
                Token = newToken,
                ExpiryDate = DateTime.UtcNow.AddDays(14),
                UserId = token.UserId
            };

            await dbContext.RefreshTokens.AddAsync(refreshToken);
            await dbContext.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<bool> ValidateRefreshTokenAsync(CookieValidatePrincipalContext context)
        {
            try
            {
                var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.Principal?.FindFirst("UserId")?.Value;

                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(email))
                {
                    return true; 
                }

                var user = await userManager.FindByEmailAsync(email);

                if (user == null || user.UserId != userId)
                {
                    context.RejectPrincipal();
                    return false;
                }

                var hasValidRefreshToken = await dbContext.RefreshTokens
                    .AnyAsync(rt => rt.UserId == userId && rt.IsActive);

                if (!hasValidRefreshToken)
                {
                    if (context.Properties.ExpiresUtc.HasValue)
                    {
                        context.RejectPrincipal();
                        return false;
                    }
                }

                var expiresUtc = context.Properties.ExpiresUtc;
                var shouldRenew = expiresUtc.HasValue && DateTime.UtcNow > expiresUtc.Value;

                if (shouldRenew)
                {
                    var isRefreshTokenExists = await dbContext.RefreshTokens
                        .AnyAsync(rt => 
                            rt.UserId == userId 
                            && rt.IsActive
                            && rt.ExpiryDate > DateTime.UtcNow);

                    if(!isRefreshTokenExists)
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
