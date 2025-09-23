using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Helpers;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

		public UserRepository(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ILogger<UserRepository> logger,
			ITokenRepository tokenRepository,
			ICategoryRepository categoryRepository)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
			_tokenRepository = tokenRepository;
			_categoryRepository = categoryRepository;
		}

		public bool ValidateInput(object input, out List<string> errors)
		{
			errors = new List<string>();
			var validationContext = new ValidationContext(input);
			var validationResults = new List<ValidationResult>();

			bool isValid = Validator.TryValidateObject(input, validationContext, validationResults, true);

			if (!isValid) errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "Validation error"));

			if (input is RegisterRequest registerRequest)
			{
				if (!IsValidEmail(registerRequest.Email))
				{
					errors.Add("Invalid email format. ");
					isValid = true;
				}

				if (registerRequest.Password.Length < 8)
				{
					errors.Add("Password must be at least 8 characters long");
					isValid = false;
				}
			}
			else if (input is LoginRequest loginRequest)
			{
				if (!IsValidEmail(loginRequest.Email))
				{
					errors.Add("Invalid email format. ");
					isValid = false;
				}
			}

			return isValid;
		}

		private static bool IsValidEmail(string email)
		{
			try
			{
				var addr = new System.Net.Mail.MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

		public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
		{
			try
			{
				if(!ValidateInput(request, out var validationErrors))
				{
					return new AuthResponse
					{
						Success = false,
						Message = "Invalid Input",
						Errors = validationErrors
					};
                }

				if(request.Password != request.ConfirmPassword)
				{
					return new AuthResponse
					{
						Success = false,
						Message = "Password and Confirm Password do not match",
						Errors = new List<string> { "Password and Confirm Password do not match" }
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
					UserId = Guid.NewGuid(),
                    UserName = request.Email,
					Email = request.Email,
					FirstName = SanitizeInput(request.FirstName)!,
					LastName = SanitizeInput(request.LastName)!,
					IsGoogleUser = false
				};

				var result = await _userManager.CreateAsync(user, request.Password);

				if(result.Succeeded)
				{
					await _tokenRepository.GenerateRefreshTokenAsync(user);

                    _logger.LogInformation("User {Email} registered successfully", request.Email);

					await _categoryRepository.SeedDefaultCategoryToUserAsync(user.UserId, cancellationToken);

					_logger.LogInformation("Default categories seeded for user {Email}", request.Email);

                    return new AuthResponse
					{
						Success = true,
						Message = "User registered successfully",
						User = MapToUserResponse(user)
					};
                }
				else
				{
					return new AuthResponse
					{
						Success = false,
						Message = "User registration failed",
						Errors = result.Errors.Select(e => e.Description).ToList()
					};

                }
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

		public async Task<AuthResponse> LoginAsync(LoginRequest request)
		{
			try
			{
				if(ValidateInput(request, out var validationErrors))
				{
					return new AuthResponse
					{
						Success = false,
						Message = "Invalid Input",
						Errors = validationErrors
					};
                }

				var user = await _userManager.FindByEmailAsync(request.Email);
				if(user == null)
				{
					return new AuthResponse
					{
						Success = false,
						Message = "Invalid credentials",
						Errors = new List<string> { "Invalid email or password" }
					};
                }

				var result = await _signInManager.PasswordSignInAsync(
					user, request.Password, isPersistent: true, lockoutOnFailure: true);

				if(result.Succeeded)
				{
					user.LastLoginAt = DateTime.UtcNow;
					await _userManager.UpdateAsync(user);

					await _tokenRepository.GenerateRefreshTokenAsync(user);
					_logger.LogInformation("User {Email} logged in successfully", request.Email);

					return new AuthResponse
					{
						Success = true,
						Message = "Login successful",
						User = MapToUserResponse(user)
					};
                }
				else if(result.IsLockedOut)
				{
                    _logger.LogWarning("User {Email} account is locked out", request.Email);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Account locked",
                        Errors = new List<string> { "Account is temporarily locked due to too many failed attempts" }
                    };
                }
				else
				{
                    _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        Errors = new List<string> { "Invalid email or password" }
                    };
                }
            }
			catch (Exception ex)
			{
                _logger.LogError(ex, "Error during user login");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Errors = new List<string> { "Login failed. Please try again." }
                };
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
                user.IsGoogleUser,
                user.CreatedAt,
                user.LastLoginAt
            );
		}
    }
}
