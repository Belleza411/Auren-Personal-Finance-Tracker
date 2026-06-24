using Auren.Application.Common.Result;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Auren.Application.Common.Interfaces
{
    public interface ITokenService
    {
        Task<Result<RefreshToken>> GetRefreshToken(Guid userId, string token, CancellationToken ct);
        Task<Result<RefreshToken>> GenerateRefreshToken(Guid userId, CancellationToken ct);
        Task<Result<RefreshToken>> RotateRefreshToken(RefreshToken oldToken, CancellationToken ct);
        Task<Auren.Application.Common.Result.Result> RevokeAllUserRefreshTokens(Guid userId, CancellationToken ct);
        Task<bool> ValidateRefreshTokenAsync(CookieValidatePrincipalContext context);
    }
}
