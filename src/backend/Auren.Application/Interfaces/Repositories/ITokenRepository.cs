
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Auren.Application.Interfaces.Repositories
{
	public interface ITokenRepository
	{
		Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user);
		string GenerateAccessTokenAsync(ApplicationUser user);
        Task<bool> ValidateRefreshTokenAsync(CookieValidatePrincipalContext context);
        Task RevokeRefreshTokenAsync(string token, string reason);
        Task RevokeAllUserRefreshTokensAsync(Guid userId);
        Task CleanupExpiredTokensAsync();
        Task RotateRefreshTokenAsync(Guid userId);
    }
}
