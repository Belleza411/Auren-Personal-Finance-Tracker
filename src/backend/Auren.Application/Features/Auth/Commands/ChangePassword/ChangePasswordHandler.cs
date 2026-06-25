using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Auth.DTOs;
using FluentValidation;

namespace Auren.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordHandler(
        IIdentityService identity,
        ITokenService token,
        IValidator<ChangePasswordDto> validator)
    {
        public async Task<Result<AuthResponse>> Handle(
            ChangePasswordCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<AuthResponse>(Error.ValidationFailed(errors));
            }

            var user = await identity.FindByIdAsync(cmd.UserId);

            if (user == null)
                return Result.Failure<AuthResponse>(Error.NotFound("User not found"));

            var result = await identity.ChangePasswordAsync(
                    user,
                    cmd.Dto.CurrentPassword,
                    cmd.Dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure<AuthResponse>(Error.ValidationFailed(errors));
            }

            await token.RevokeAllUserRefreshTokens(cmd.UserId, ct);

            return Result.Success(new AuthResponse
            {
                Success = true
            });
        }
    }
}
