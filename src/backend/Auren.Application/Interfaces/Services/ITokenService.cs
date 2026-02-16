using Auren.Application.Common.Result;
using Auren.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<Result<RefreshToken>> GetRefreshToken(Guid userId, string token);
        Task<Result<RefreshToken>> GenerateRefreshToken(Guid userId);
        Task<Result<RefreshToken>> RotateRefreshToken(RefreshToken token);
        Task<Result> RevokeAllUserRefreshTokens(Guid userId);
    }
}
