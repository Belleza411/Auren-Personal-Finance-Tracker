using Auren.Application.Common.Result;
using Auren.Application.Constants;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.User;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IValidator<RegisterRequest> _registerValidator;
		private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IProfileRepository _profileRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenRepository _tokenRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

		public UserService(IUserRepository userRepository,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IProfileRepository profileRepository,
            ICategoryRepository categoryRepository,
            ILogger<UserService> logger,
            ITokenRepository tokenRepository,
            IHttpContextAccessor httpContextAccessor)
		{
			_userRepository = userRepository;
			_registerValidator = registerValidator;
			_loginValidator = loginValidator;
			_profileRepository = profileRepository;
			_categoryRepository = categoryRepository;
			_logger = logger;
			_tokenRepository = tokenRepository;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<AuthResponse>(Error.ValidationFailed(errors));
            }

            var exisitingUser = await _userRepository.FindEmailAsync(request.Email);
            if (exisitingUser != null)
                return Result.Failure<AuthResponse>(Error.UserError.EmailAlreadyInUse("Email already exists. "));

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = SanitizeInput(request.FirstName)!,
                LastName = SanitizeInput(request.LastName)!,
            };

            if (request.ProfileImage != null)
            {
                var imageUploadResult = await TryUploadProfileImageAsync(
                    request.ProfileImage,
                    request.FirstName,
                    request.LastName,
                    cancellationToken
                );

                if (!imageUploadResult.Success)
                    return Result.Failure<AuthResponse>(Error.UploadFailed(imageUploadResult.ErrorMessage ?? "Failed to upload image."));

                user.ProfilePictureUrl = imageUploadResult.ImagePath;
            }

            var result = await _userRepository.CreateUserAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure<AuthResponse>(Error.CreateFailed(errors));
            }

            await TrySeedDefaultCategoriesAsync(user.Id, cancellationToken);
            return await SignUserInAsync(user, "Registered successfully");
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<AuthResponse>(Error.ValidationFailed(errors));
            }

            var user = await _userRepository.FindEmailAsync(request.Email);
            if (user == null)
                return Result.Failure<AuthResponse>(Error.InvalidInput("Invalid email or password."));

            var result = await _userRepository.CheckPasswordAsync(user, request.Password);

            if (result.Succeeded)
            {
                return await HandleSuccessfulLogin(user, request.Email);
            }

            if (result.IsLockedOut)
            {
                var lockedOutResponse = await HandleLockedOutAccountAsync(user, request.Email);
                return Result.Failure<AuthResponse>(Error.UserError.UserLockedOut(lockedOutResponse.Message));
            }

            var failedLoginResponse = await HandleFailedLoginAsync(user, request.Email);
            return Result.Failure<AuthResponse>(Error.InvalidInput(failedLoginResponse.Message));
        }

        public async Task<Result<bool>> LogoutAsync(CancellationToken cancellationToken)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User?
                    .FindFirst("UserId")?.Value;

                if (userId == null)
                    return Result.Failure<bool>(Error.InvalidInput("No active session or invalid user id"));

                if(!string.IsNullOrEmpty(userId) && !Guid.TryParse(userId, out var parsedUserId))
                    await _tokenRepository.RevokeAllUserRefreshTokensAsync(parsedUserId);

                await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return Result.Failure<bool>(Error.UserError.LogoutFailed("Logout failed."));
            }
        }

        private static string? SanitizeInput(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            return System.Net.WebUtility.HtmlEncode(input.Trim());
        }

        private async Task<(bool Success, string? ImagePath, string? ErrorMessage)> TryUploadProfileImageAsync(
            ProfileImageUploadRequest profileImage,
            string firstName,
            string lastName,
            CancellationToken cancellationToken)
        {
            try
            {
                var uploadRequest = new ProfileImageUploadRequest(
                    File: profileImage.File,
                    Name: Path.GetFileNameWithoutExtension(profileImage.File.FileName),
                    Description: string.IsNullOrWhiteSpace(profileImage.Description)
                        ? $"{firstName} {lastName}"
                        : profileImage.Description
                );

                var uploadResponse = await _profileRepository.UploadProfileImageAsync(
                    uploadRequest,
                    cancellationToken
                );

                if (uploadResponse == null)
                    return (false, null, "Profile image upload returned null response.");

                return (true, uploadResponse.Path, null);
            }
            catch (Exception)
            {
                return (false, null, "Failed to upload profile image.");
            }
        }

        private async Task TrySeedDefaultCategoriesAsync(Guid userId, CancellationToken cancellationToken)
        {
            var categories = CategorySeeder.DefaultCategories.Select(c => new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = c.Name,
                TransactionType = c.transactionType,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            try
            {
                await _categoryRepository.SeedDefaultCategoryToUserAsync(categories, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to seed default categories for user {UserId}", userId);
            }
        }
        private static UserResponse MapToUserResponse(ApplicationUser user)
        {
            return new UserResponse(
                user.Email!,
                user.FirstName,
                user.LastName,
                user.ProfilePictureUrl,
                user.CreatedAt,
                user.LastLoginAt
            );
        }
        private async Task<Result<AuthResponse>> HandleSuccessfulLogin(ApplicationUser user, string email)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("Successful login for user: {Email}", email);

            return await SignUserInAsync(user, "Login successfully");
        }

        private async Task<AuthResponse> HandleLockedOutAccountAsync(ApplicationUser user, string email)
        {
            var lockoutEnd = await _userRepository.GetLockoutEndDateAsync(user);
            var remainingTime = lockoutEnd?.Subtract(DateTimeOffset.UtcNow);

            _logger.LogWarning(
                "Login attempt for locked out user: {Email}. Lockout ends at: {LockoutEnd}",
                email,
                lockoutEnd
            );

            var errorMessage = remainingTime.HasValue && remainingTime.Value.TotalMinutes > 0
                ? $"Account is temporarily locked due to multiple failed login attempts. Please try again in {Math.Ceiling(remainingTime.Value.TotalMinutes)} minutes."
                : "Account is temporarily locked due to multiple failed login attempts";

            return CreateErrorResponse("Account Locked", errorMessage);
        }

        private async Task<AuthResponse> HandleFailedLoginAsync(ApplicationUser user, string email)
        {
            var failedAttempts = await _userRepository.GetAccessFailedCountAsync(user);

            _logger.LogWarning(
                "Failed login attempt for user: {Email}. Failed attempts: {FailedAttempts}",
                email,
                failedAttempts
            );

            return CreateErrorResponse("Invalid credentials", "Invalid email or password");
        }

        private static AuthResponse CreateErrorResponse(string message, params string[] errors)
        {
            return new AuthResponse
            {
                Success = false,
                Message = message,
                Errors = [.. errors]
			};
        }
        private async Task<Result<AuthResponse>> SignUserInAsync(ApplicationUser user, string? message)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new("UserId", user.UserId.ToString()),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new("UserId", user.UserId.ToString()),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var refreshToken = await _tokenRepository.GenerateRefreshTokenAsync(user);

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    IssuedUtc = DateTimeOffset.UtcNow,
                    AllowRefresh = true
                });

            return Result.Success<AuthResponse>(new AuthResponse
            {
                Success = true,
                Message = message!,
                User = MapToUserResponse(user),
            });
        }
    }
}
