using Auren.API.Models.Domain;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Auren.API.Repositories.Interfaces
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
