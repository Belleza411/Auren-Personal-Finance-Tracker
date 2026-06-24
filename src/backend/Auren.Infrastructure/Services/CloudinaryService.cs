using Auren.Application.Features.Profile.DTOs;
using Auren.Infrastructure.Common.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Auren.Infrastructure.Services
{
    public class CloudinaryService(
        Cloudinary cloudinary,
        IValidator<ProfileImageUploadRequest> validator
        ) : ICloudinaryService
    {
        public async Task<ProfileImageUploadResponse> UploadProfileImageAsync(
            ProfileImageUploadRequest request,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Exception(errors);
            }

            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            var safeName = Path.GetFileNameWithoutExtension(request.Name ?? "");
            var fileName = string.IsNullOrWhiteSpace(safeName)
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

            var uploadResult = await cloudinary.UploadAsync(uploadParams, ct);

            if (uploadResult == null || uploadResult.StatusCode != HttpStatusCode.OK)
                throw new Exception("Cloudinary upload failed.");

            return new ProfileImageUploadResponse(
                Name: fileName,
                Description: request.Description,
                Extension: extension,
                SizeInBytes: request.File.Length,
                Path: uploadResult.SecureUrl.ToString()
            );
        }
    }
}
