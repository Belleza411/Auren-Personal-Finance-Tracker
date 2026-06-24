using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Features.Auth.DTOs;
using Auren.Application.Features.Auth.Helper;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Auren.Application.Features.Auth.Commands.Login
{
    public class LoginHandler(
        IIdentityService identity,
        ITokenService tokenService,
        IHttpContextAccessor http,
        IValidator<LoginRequest> validator)
    {
        public async Task<Result<AuthResponse>> Handle(
            LoginCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Request, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<AuthResponse>(Error.ValidationFailed(errors));
            }

            var user = await identity.FindByEmailAsync(cmd.Request.Email);
            if (user == null)
                return Result.Failure<AuthResponse>(Error.InvalidInput("Invalid email or password."));

            var result = await identity.CheckPasswordAsync(user, cmd.Request.Password);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await identity.UpdateAsync(user);
                return await AuthHelper.SignInAsync(user, "Login successfully", tokenService, http, ct);
            }

            if (result.IsLockedOut)
            {
                var lockoutEnd = await identity.GetLockoutEndDateAsync(user);
                var remaining = lockoutEnd?.Subtract(DateTimeOffset.UtcNow);
                var message = remaining?.TotalMinutes > 0
                    ? $"Account locked. Try again in {Math.Ceiling(remaining.Value.TotalMinutes)} minutes."
                    : "Account is temporarily locked.";
                return Result.Failure<AuthResponse>(Error.UserError.UserLockedOut(message));
            }

            return Result.Failure<AuthResponse>(Error.InvalidInput("Invalid email or password."));
        }
    }
}
