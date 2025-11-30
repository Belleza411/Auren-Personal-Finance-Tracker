using Auren.Application.Common.Result;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Application.Validators;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.Services
{
	public class ProfileService : IProfileService
	{
		private readonly IValidator<ProfileImageUploadRequest> _profileImageUploadValidator;
		private readonly IValidator<UserDto> _userValidator;
		private readonly IProfileRepository _profileRepository;
		private readonly IUserRepository _userRepository;

		public ProfileService(IValidator<ProfileImageUploadRequest> profileImageUploadValidator, IValidator<UserDto> userValidator, IProfileRepository profileRepository, IUserRepository userRepository)
		{
			_profileImageUploadValidator = profileImageUploadValidator;
			_userValidator = userValidator;
			_profileRepository = profileRepository;
			_userRepository = userRepository;
		}

		public async Task<Result<UserResponse>> GetUserProfile(Guid userId, CancellationToken cancellationToken)
		{
			var userProfile = await _profileRepository.GetUserProfileAsync(userId, cancellationToken);
			return userProfile == null ? Result.Failure<UserResponse>(Error.NotFound("User not found.")) : Result.Success(userProfile);
        }

		public async Task<Result<UserResponse>> UpdateUserProfile(Guid userId, UserDto userDto, CancellationToken cancellationToken)
		{
			var validationResult = await _userValidator.ValidateAsync(userDto, cancellationToken);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
				return Result.Failure<UserResponse>(Error.ValidationFailed(errors));
			}

			var user = await _userRepository.GetUserById(userId, cancellationToken);

			if (user == null)
				return Result.Failure<UserResponse>(Error.NotFound("User not found."));

			if (userDto.ProfilePictureUrl?.File != null)
			{
				var uploadRequest = new ProfileImageUploadRequest(
					File: userDto.ProfilePictureUrl.File,
					Name: Path.GetFileNameWithoutExtension(userDto.ProfilePictureUrl.File.FileName),
					Description: string.IsNullOrEmpty(userDto.ProfilePictureUrl.Description)
						? $"{userDto.FirstName}{userDto.LastName}"
						: userDto.ProfilePictureUrl.Description
				);

				var uploadResponse = await _profileRepository.UploadProfileImageAsync(uploadRequest, cancellationToken);

				if (uploadResponse != null && !string.IsNullOrEmpty(uploadResponse?.Path)) 
					user.ProfilePictureUrl = uploadResponse.Path ?? user.ProfilePictureUrl;
            }

			user.Email = userDto.Email ?? user.Email;
			user.FirstName = userDto.FirstName ?? user.FirstName;
			user.LastName = userDto.LastName ?? user.LastName;
			user.Currency = userDto.Currency ?? user.Currency;

			var updatedProfile = await _profileRepository.UpdateUserProfileAsync(userId, user, cancellationToken);

			return updatedProfile == null 
				? Result.Failure<UserResponse>(Error.UpdateFailed("Failed to update profile.")) 
				: Result.Success<UserResponse>(updatedProfile);
        }
    }
}
