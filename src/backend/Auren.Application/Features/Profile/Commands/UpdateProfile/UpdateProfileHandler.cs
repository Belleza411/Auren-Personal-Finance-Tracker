using Auren.Application.Common.Interfaces; 
using Auren.Application.Common.Result;
using Auren.Application.Features.Auth.DTOs;
using Auren.Application.Features.Profile.DTOs;
using Auren.Domain.Entities;
using Auren.Infrastructure.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Features.Profile.Commands.UpdateProfile
{
    public class UpdateProfileHandler(
        IAuthDbContext db,
        ICloudinaryService cloudinary,
        IValidator<UserDto> validator)
    {
        public async Task<Result<UserResponse>> Handle(
            UpdateProfileCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Dto, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<UserResponse>(Error.ValidationFailed(errors));
            }

            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == cmd.UserId, ct);

            if (user == null)
                return Result.Failure<UserResponse>(Error.NotFound("User not found."));

            if (cmd.Dto.ProfilePictureUrl?.File != null)
            {
                var uploadRequest = new ProfileImageUploadRequest(
                    File: cmd.Dto.ProfilePictureUrl.File,
                    Name: Path.GetFileNameWithoutExtension(cmd.Dto.ProfilePictureUrl.File.FileName),
                    Description: string.IsNullOrEmpty(cmd.Dto.ProfilePictureUrl.Description)
                        ? $"{cmd.Dto.FirstName}{cmd.Dto.LastName}"
                        : cmd.Dto.ProfilePictureUrl.Description
                );

                var uploadResponse = await cloudinary.UploadProfileImageAsync(uploadRequest, ct);

                if (!string.IsNullOrEmpty(uploadResponse?.Path))
                    user.ProfilePictureUrl = uploadResponse.Path;

                user.Email = cmd.Dto.Email ?? user.Email;
                user.FirstName = cmd.Dto.FirstName ?? user.FirstName;
                user.LastName = cmd.Dto.LastName ?? user.LastName;
                user.Currency = cmd.Dto.Currency ?? user.Currency;

                await db.SaveChangesAsync(ct);

                return Result.Success(MapToUserResponse(user));
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
    }
}
