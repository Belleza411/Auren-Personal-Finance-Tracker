using Auren.API.Data;
using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auren.API.Repositories.Implementations
{
	public class ProfileRepository : IProfileRepository
	{
		private readonly ILogger<ProfileRepository> _logger;
		private readonly AurenAuthDbContext _dbContext;

		public ProfileRepository(ILogger<ProfileRepository> logger, AurenAuthDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
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
				user.IsGoogleUser,
				user.CreatedAt,
				user.LastLoginAt
			);
		}
    }
}
