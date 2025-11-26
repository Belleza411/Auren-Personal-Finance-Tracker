using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;


namespace Auren.API.Repositories.Implementations
{
	public class UserRepository : IUserRepository
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger<UserRepository> _logger;
		private readonly ITokenRepository _tokenRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly IProfileRepository _profileRepository;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

		public UserRepository(UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ILogger<UserRepository> logger,
			ITokenRepository tokenRepository,
			ICategoryRepository categoryRepository,
			IProfileRepository profileRepository,
			IValidator<RegisterRequest> registerValidator,
			IValidator<LoginRequest> loginValidator)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
			_tokenRepository = tokenRepository;
			_categoryRepository = categoryRepository;
			_profileRepository = profileRepository;
			_registerValidator = registerValidator;
			_loginValidator = loginValidator;
		}

		public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
		{
			try
			{
				var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
				if(!validationResult.IsValid)
				{
					return new AuthResponse
					{
						Success = false,
						Message = "Invalid Input",
						Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
					};
                }

				var exisitingUser = await _userManager.FindByEmailAsync(request.Email);
				if(exisitingUser != null)
				{
					return new AuthResponse
					{
						Success = false,
						Message = "User already exists",
						Errors = new List<string> { "User with this email already exists" }
					};
				}

				var user = new ApplicationUser
				{
                    UserName = request.Email,
					Email = request.Email,
					FirstName = SanitizeInput(request.FirstName)!,
					LastName = SanitizeInput(request.LastName)!,
				};

				if(request.ProfileImage != null)
				{
                    var imageUploadResult = await TryUploadProfileImageAsync(
					   request.ProfileImage,
					   request.FirstName,
					   request.LastName,
					   request.Email,
					   cancellationToken
					);

					if(!imageUploadResult.Success)
					{
						return new AuthResponse
						{
							Success = false,
							Message = "Profile image upload failed",
							Errors = new List<string> { imageUploadResult.ErrorMessage ?? "Unknown error" }
						};
					}

					user.ProfilePictureUrl = imageUploadResult.ImagePath;
                }

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "User registration failed",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                await TrySeedDefaultCategoriesAsync(user.Id, cancellationToken);

                return new AuthResponse
                {
                    Success = true,
                    Message = "User registered successfully",
                    User = MapToUserResponse(user)
                };
            }
			catch (Exception ex)
			{
                _logger.LogError(ex, "Error during user registration");
				return new AuthResponse
				{
					Success = false,
					Message = "An error occurred during registration",
					Errors = new List<string> { "Registration failed. Please try again." }
				};
            }
        }

		public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
		{
			try
			{
				var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid Input",
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    };
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
				if(user == null)
				{
                    _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
                    return CreateErrorResponse(
						"Invalid Input",
                        validationResult.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                }

				var result = await _signInManager.CheckPasswordSignInAsync(
					user,
					request.Password, 
					lockoutOnFailure: true
				);

				if(result.Succeeded)
				{
					return await HandleSuccessfulLoginAsync(user, request.Email);
                }
				
				if(result.IsLockedOut)
				{
					return await HandleLockedOutAccountAsync(user, request.Email);
				}


				return await HandleFailedLoginAsync(user, request.Email);
            }
			catch (Exception ex)
			{
                _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
                return CreateErrorResponse("Login error", "Login failed. Please try again later.");
            }
        }

		private static string? SanitizeInput(string? input)
		{
			if (string.IsNullOrWhiteSpace(input)) return null;
            return System.Net.WebUtility.HtmlEncode(input.Trim());
        }

		private static UserResponse MapToUserResponse(ApplicationUser user)
		{
			return new UserResponse(
				user.UserId,
				user.Email!,
                user.FirstName,
                user.LastName,
                user.ProfilePictureUrl,
                user.CreatedAt,
                user.LastLoginAt
            );
		}

        private async Task<(bool Success, string? ImagePath, string? ErrorMessage)> TryUploadProfileImageAsync(
			ProfileImageUploadRequest profileImage,
			string firstName,
			string lastName,
			string email,
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

                return (true, uploadResponse.Path, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile image upload failed for {Email}", email);
                return (false, null, "Failed to upload profile image.");
            }
        }

        private async Task TrySeedDefaultCategoriesAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                await _categoryRepository.SeedDefaultCategoryToUserAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to seed default categories for user {UserId}", userId);
            }
        }
    
		private static AuthResponse CreateErrorResponse(string message, params string[] errors)
		{
			return new AuthResponse
			{
				Success = false,
				Message = message,
				Errors = errors.ToList()
			};
        }
		
		private async Task<AuthResponse> HandleSuccessfulLoginAsync(ApplicationUser user, string email)
		{
			try
			{
				user.LastLoginAt = DateTime.UtcNow;
				await _userManager.UpdateAsync(user);
				
                _logger.LogInformation("Successful login for user: {Email}", email);

                return new AuthResponse
				{
					Success = true,
					Message = "Login successful",
					User = MapToUserResponse(user)
				};

            }
            catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating user login timestamp for {Email}", email);
				return new AuthResponse
				{
					Success = true,
					Message = "Login successful, but failed to update last login time",
					User = MapToUserResponse(user)
				};
            }
		}
	
		private async Task<AuthResponse> HandleLockedOutAccountAsync(ApplicationUser user, string email)
		{
			var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
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
            var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);

            _logger.LogWarning(
                "Failed login attempt for user: {Email}. Failed attempts: {FailedAttempts}",
                email,
                failedAttempts
            );

            return CreateErrorResponse("Invalid credentials", "Invalid email or password");
        }
    }
}
