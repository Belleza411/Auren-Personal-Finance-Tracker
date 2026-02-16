using Auren.Application.Common.Result;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Services
{
    public class TokenService(ITokenRepository tokenRepository) : ITokenService
    {
        public async Task<Result<RefreshToken>> GetRefreshToken(Guid userId, string token)
        {
            var refreshToken = await tokenRepository.GetRefreshTokenAsync(userId, token);

            if (refreshToken == null)
            {
                return Result.Failure<RefreshToken>(Error.NotFound("Refresh token not found."));
            }

            return Result<RefreshToken>.Success(refreshToken);
        }

        public async Task<Result<RefreshToken>> GenerateRefreshToken(Guid userId)
        {
            var refreshToken = await tokenRepository.GenerateRefreshTokenAsync(userId);

            if (refreshToken == null)
            {
                return Result.Failure<RefreshToken>(Error.CreateFailed("Failed to generate refresh token."));
            }
            return Result<RefreshToken>.Success(refreshToken);
        }

        public async Task<Result<RefreshToken>> RotateRefreshToken(RefreshToken token)
        {
            var newToken = await tokenRepository.RotateRefreshTokenAsync(token);

            if (newToken == null)
            {
                return Result.Failure<RefreshToken>(Error.CreateFailed("Failed to rotate refresh token."));
            }

            return Result<RefreshToken>.Success(newToken);
        }

        public async Task<Result> RevokeAllUserRefreshTokens(Guid userId)
        {
            await tokenRepository.RevokeAllUserRefreshTokensAsync(userId);
            return Result.Success();
        }
    }
}
