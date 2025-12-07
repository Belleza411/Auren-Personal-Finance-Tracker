using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Auren.Infrastructure.Repositories
{
	public class TokenRepository(
		AurenAuthDbContext dbContext,
		UserManager<ApplicationUser> userManager) 
		: ITokenRepository
	{
		public async Task CleanupExpiredTokensAsync()
		{
			var expiredTokens = await dbContext.RefreshTokens
				.Where(rt => rt.ExpiryDate < DateTime.UtcNow)
				.ToListAsync();

			dbContext.RefreshTokens.RemoveRange(expiredTokens);
			await dbContext.SaveChangesAsync();
        }

		public string GenerateAccessTokenAsync(ApplicationUser user)
		{
			var tokenBytes = new byte[32];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(tokenBytes);
			var token = Convert.ToBase64String(tokenBytes);
			return token;
        }

		public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user)
		{
			await RevokeAllUserRefreshTokensAsync(user.UserId);

            var tokenBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes);

			var refreshToken = new RefreshToken
			{
				Token = token,
				ExpiryDate = DateTime.UtcNow.AddDays(14),
				UserId = user.UserId
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

				await dbContext.SaveChangesAsync();
            }
		}

		public async Task RotateRefreshTokenAsync(Guid userId)
		{
			var currentToken = await dbContext.RefreshTokens
				.FirstOrDefaultAsync(rt => rt.UserId == userId && rt.IsActive);

			if (currentToken != null)
			{
				var user = await userManager.Users
					.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
				{
					var newToken = await GenerateRefreshTokenAsync(user);

                    await dbContext.SaveChangesAsync();
                }
            }
		}

		public async Task<bool> ValidateRefreshTokenAsync(CookieValidatePrincipalContext context)
		{
			try
			{
				var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				
                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email) || !Guid.TryParse(userIdClaim, out var userId))
				{
                    context.RejectPrincipal();
                    return false;
                }

                if (string.IsNullOrEmpty(email))
                {
                    context.RejectPrincipal();
                    return false;
                }

                var user = await userManager.FindByEmailAsync(email);

				if(user == null || user.UserId != userId)
				{
                    context.RejectPrincipal();
                    return false;
                }

				var hasValidRefreshToken = await dbContext.RefreshTokens
					.AnyAsync(rt => rt.UserId == userId && rt.IsActive);

				if(!hasValidRefreshToken)
				{
                    context.RejectPrincipal();
                    return false;
                }

				var expiresUtc = context.Properties.ExpiresUtc;
				var shouldRenew = expiresUtc.HasValue && DateTime.UtcNow > expiresUtc.Value.AddMinutes(-2);

                if(shouldRenew)
				{
					var newAccessToken = GenerateAccessTokenAsync(user);

					var identity = (ClaimsIdentity)context.Principal?.Identity!;
					var existingTokenClaim = identity.FindFirst("AccessToken");

					if (existingTokenClaim != null)
					{
						identity.RemoveClaim(existingTokenClaim);
                    }

					identity.AddClaim(new Claim("AccessToken", newAccessToken));

					await RotateRefreshTokenAsync(userId);

                    context.Properties.IssuedUtc = DateTimeOffset.UtcNow;
                    context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10);
                    context.ShouldRenew = true;
                }

                return true;
            }
			catch (Exception)
			{
                context.RejectPrincipal();
                return false;
            }
		}
	}
}
