using Auren.API.Data;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Auren.API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Auren.API.Repositories.Implementations
{
	public class ProfileRepository : IProfileRepository
	{
		private readonly ILogger<ProfileRepository> _logger;
		private readonly AurenAuthDbContext _dbContext;
		private readonly IWebHostEnvironment _env;
		private readonly IValidator<ProfileImageUploadRequest> _validator;
        private readonly IOptions<FileUploadSettings> _fileUploadSettings;

		public ProfileRepository(ILogger<ProfileRepository> logger,
			AurenAuthDbContext dbContext,
			IWebHostEnvironment env,
			IValidator<ProfileImageUploadRequest> validator,
			IOptions<FileUploadSettings> fileUploadSettings)
		{
			_logger = logger;
			_dbContext = dbContext;
			_env = env;
			_validator = validator;
			_fileUploadSettings = fileUploadSettings;
		}

		public async Task<UserResponse?> GetUserProfile(Guid userId, CancellationToken cancellationToken)
		{
			try
			{
				var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
				{
					_logger.LogWarning("User with ID {UserId} not found", userId);
					return null;
                }

				return MapToUserResponse(user);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request cancelled while retrieving profile for user {UserId}", userId);
                throw;
            }
            catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve profile for user {UserId}", userId);
				throw;
            }
        }

		public async Task<UserResponse?> UpdateUserProfile(Guid userId, UserDto userDto, CancellationToken cancellationToken)
		{
            if (userDto == null)
            {
                throw new ArgumentNullException(nameof(userDto));
            }

            try
			{
                var user = await _dbContext.Users
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

				if(user == null)
				{
					_logger.LogWarning("User with ID {UserId} not found for update", userId);
					return null;
                }

				user.Email = userDto.Email;
                user.FirstName = userDto.FirstName!;
				user.LastName = userDto.LastName!;
				user.ProfilePictureUrl = userDto.ProfilePictureUrl;
				user.Currency = userDto.Currency;

				await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User with ID {UserId} successfully updated", userId);

                return MapToUserResponse(user);

            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to update profile for user {UserId}", userId);
				throw;
            }
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

		public async Task<ProfileImageUploadResponse> UploadProfileImageAsync(ProfileImageUploadRequest request, CancellationToken cancellationToken)
		{
			try
			{
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if ((!validationResult.IsValid))
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new Exception(errors);
                }

                var uploadsFolder = Path.Combine(_env.ContentRootPath, _fileUploadSettings.Value.ProfileImagesPath);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

                var safeName = Path.GetFileNameWithoutExtension(request.Name ?? "");
                string fileName = string.IsNullOrWhiteSpace(safeName)
                    ? $"{Guid.NewGuid()}{extension}"
                    : $"{safeName}{extension}";

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, cancellationToken);
                }

                return new ProfileImageUploadResponse(
                    Name: fileName,
                    Description: request.Description,
                    Extension: extension,
                    SizeInBytes: request.File.Length,
                    Path: $"{_fileUploadSettings.Value.BaseUrl}   {fileName}"
                );
            } 
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to upload profile image for user");
				throw;
            }
        }
    }
}
