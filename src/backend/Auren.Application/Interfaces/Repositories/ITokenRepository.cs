
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Auren.Application.Interfaces.Repositories
{
	public interface ITokenRepository
	{
        Task<RefreshToken?> GetRefreshTokenAsync(Guid userId, string token);
        Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId);
		string GenerateAccessTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(CookieValidatePrincipalContext context);
        Task RevokeRefreshTokenAsync(string token, string reason);
        Task RevokeAllUserRefreshTokensAsync(Guid userId);
        Task CleanupExpiredTokensAsync();
        Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken token);
    }
}
