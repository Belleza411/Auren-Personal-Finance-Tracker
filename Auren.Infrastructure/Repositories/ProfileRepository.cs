using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.User;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Auren.Infrastructure.Repositories
{
	public class ProfileRepository : IProfileRepository
	{
		private readonly ILogger<ProfileRepository> _logger;
		private readonly AurenAuthDbContext _dbContext;
		private readonly IValidator<ProfileImageUploadRequest> _validator;
		private readonly Cloudinary _cloudinary;

		public ProfileRepository(ILogger<ProfileRepository> logger, AurenAuthDbContext dbContext, IValidator<ProfileImageUploadRequest> validator, Cloudinary cloudinary)
		{
			_logger = logger;
			_dbContext = dbContext;
			_validator = validator;
			_cloudinary = cloudinary;
		}

		public async Task<UserResponse?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken)
		{
			var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
			{
				_logger.LogWarning("User with ID {UserId} not found", userId);
				return null;
            }

			return MapToUserResponse(user);
        }

		public async Task<UserResponse?> UpdateUserProfileAsync(Guid userId, ApplicationUser user, CancellationToken cancellationToken)
		{
			_dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User with ID {UserId} successfully updated", userId);

            return MapToUserResponse(user);
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

                var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

                var safeName = Path.GetFileNameWithoutExtension(request.Name ?? "");
                string fileName = string.IsNullOrWhiteSpace(safeName)
                    ? $"{Guid.NewGuid()}{extension}"
                    : $"{safeName}{extension}";

				using var stream = request.File.OpenReadStream();

				var uploadParams = new ImageUploadParams
				{
					File = new FileDescription(fileName, stream),
					PublicId = Path.GetFileNameWithoutExtension(fileName),
					Folder = "profile-images",
					Overwrite = true,
				};

				var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

				if(uploadResult == null || uploadResult.StatusCode != HttpStatusCode.OK)
					throw new Exception("Clooudinary upload failed");

                return new ProfileImageUploadResponse(
                    Name: fileName,
                    Description: request.Description,
                    Extension: extension,
                    SizeInBytes: request.File.Length,
                    Path: uploadResult.SecureUrl.ToString()
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
