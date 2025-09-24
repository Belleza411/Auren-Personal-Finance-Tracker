using Auren.API.Data;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Auren.API.Repositories.Implementations
{
	public class TokenRepository : ITokenRepository
	{
		private readonly AurenAuthDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger<TokenRepository> _logger;

		public TokenRepository(AurenAuthDbContext dbContext,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ILogger<TokenRepository> logger)
		{
			_dbContext = dbContext;
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
		}

		public async Task CleanupExpiredTokensAsync()
		{
			var expiredTokens = await _dbContext.RefreshTokens
				.Where(rt => rt.ExpiryDate < DateTime.UtcNow)
				.ToListAsync();

			_dbContext.RefreshTokens.RemoveRange(expiredTokens);
			await _dbContext.SaveChangesAsync();

			_logger.LogInformation("Cleaned up {Count} expired refresh tokens.", expiredTokens.Count);
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

            var tokenBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            var token = Convert.ToBase64String(tokenBytes);

			var refreshToken = new RefreshToken
			{
				Token = token,
				ExpiryDate = DateTime.UtcNow.AddDays(14),
				UserId = user.UserId
            };

			await _dbContext.RefreshTokens.AddAsync(refreshToken);
			await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Generated new refresh token for user {UserId}", user.Id);

            return refreshToken;
        }

		public async Task RevokeAllUserRefreshTokensAsync(Guid userId)
		{
			var refreshTokens = await _dbContext.RefreshTokens
				.Where(rt => rt.UserId == userId && rt.IsActive)
				.ToListAsync();


			foreach (var token in refreshTokens)
			{
				token.IsRevoked = true;
				token.RevokedAt = DateTime.UtcNow;
				token.ReasonRevoked = "User logged out";
            }

			if(refreshTokens.Any())
			{
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}",
					refreshTokens.Count, userId);
            }
        }

		public async Task RevokeRefreshTokenAsync(string token, string reason)
		{
			var refreshToken = await _dbContext.RefreshTokens
				.FirstOrDefaultAsync(rt => rt.Token == token);

			if(refreshToken != null && refreshToken.IsActive)
			{
				refreshToken.IsRevoked = true;
				refreshToken.RevokedAt = DateTime.UtcNow;
				refreshToken.ReasonRevoked = reason;

				await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Revoked refresh token for user {UserId}: {Reason}",
                                    refreshToken.UserId, reason);
            }
		}

		public async Task RotateRefreshTokenAsync(Guid userId)
		{
			var currentToken = await _dbContext.RefreshTokens
				.FirstOrDefaultAsync(rt => rt.UserId == userId && rt.IsActive);

			if (currentToken != null)
			{
				var user = await _userManager.Users
					.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user != null)
				{
					var newToken = await GenerateRefreshTokenAsync(user);

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Rotated refresh token for user {UserId}", userId);
                }
            }
		}

		public async Task<bool> ValidateRefreshTokenAsync(CookieValidatePrincipalContext context)
		{
			try
			{
				var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				
                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;

                _logger.LogDebug("Validating token for email: {Email}, UserId: {UserId}", email, userIdClaim);

                if (string.IsNullOrEmpty(email) || !Guid.TryParse(userIdClaim, out var userId))
				{
                    _logger.LogWarning("Invalid or missing UserId claim");
                    context.RejectPrincipal();
                    return false;
                }

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Missing email claim");
                    context.RejectPrincipal();
                    return false;
                }

                var user = await _userManager.FindByEmailAsync(email);
                _logger.LogInformation("Email found: {Userl}", user);

				if(user == null || user.UserId != userId)
				{
                    _logger.LogError("User not found {User}", user);
                    context.RejectPrincipal();
                    return false;
                }

				var hasValidRefreshToken = await _dbContext.RefreshTokens
					.AnyAsync(rt => rt.UserId == userId && rt.IsActive);

				if(!hasValidRefreshToken)
				{
                    _logger.LogWarning("No valid refresh token found for user {UserId}", userId);
                    context.RejectPrincipal();
                    return false;
                }

				var expiresUtc = context.Properties.ExpiresUtc;
				var shouldRenew = expiresUtc.HasValue && DateTime.UtcNow > expiresUtc.Value.AddMinutes(-2);

                if(shouldRenew)
				{
                    _logger.LogInformation("Renewing access token for user {UserId}", userId);

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

                    _logger.LogInformation("Successfully renewed tokens for user {UserId}", userId);
                }

                return true;
            }
			catch (Exception ex)
			{
                _logger.LogError(ex, "Error validating refresh token");
                context.RejectPrincipal();
                return false;
            }
		}
	}
}
