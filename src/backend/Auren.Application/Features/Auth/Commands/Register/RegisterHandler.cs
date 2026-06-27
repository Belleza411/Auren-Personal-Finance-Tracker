using Auren.Application.Common.Interfaces;
using Auren.Application.Common.Result;
using Auren.Application.Constants;
using Auren.Application.Features.Auth.DTOs;
using Auren.Application.Features.Auth.Helper;
using Auren.Application.Features.Profile.DTOs;
using Auren.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Auren.Application.Features.Auth.Commands.Register
{
    public class RegisterHandler(
        IIdentityService identity,  
        IAppDbContext appDb,        
        ICloudinaryService cloudinary,
        ITokenService tokenService,
        IHttpContextAccessor http,
        IValidator<RegisterRequest> validator)
    {
        public async Task<Result<AuthResponse>> Handle(
            RegisterCommand cmd,
            CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(cmd.Request, ct);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
                return Result.Failure<AuthResponse>(Error.ValidationFailed(errors));
            }

            var existing = await identity.FindByEmailAsync(cmd.Request.Email);
            if (existing != null)
                return Result.Failure<AuthResponse>(Error.UserError.EmailAlreadyInUse("Email already exists."));

            var user = new ApplicationUser
            {
                UserName = cmd.Request.Email,
                Email = cmd.Request.Email,
                FirstName = SanitizeInput(cmd.Request.FirstName)!,
                LastName = SanitizeInput(cmd.Request.LastName)!,
            };

            if (cmd.Request.ProfileImage != null)
            {
                var upload = await cloudinary.UploadProfileImageAsync(new ProfileImageUploadRequest(
                    File: cmd.Request.ProfileImage.File,
                    Name: Path.GetFileNameWithoutExtension(cmd.Request.ProfileImage.File.FileName),
                    Description: string.IsNullOrWhiteSpace(cmd.Request.ProfileImage.Description)
                        ? $"{cmd.Request.FirstName} {cmd.Request.LastName}"
                        : cmd.Request.ProfileImage.Description
                ), ct);

                if (upload == null)
                    return Result.Failure<AuthResponse>(Error.UploadFailed("Failed to upload image."));

                user.ProfilePictureUrl = upload.Path;
            }

            var result = await identity.CreateAsync(user, cmd.Request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return Result.Failure<AuthResponse>(Error.CreateFailed(errors));
            }

            var categories = CategorySeeder.DefaultCategories.Select(c => new Category
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = c.Name,
                TransactionType = c.transactionType,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await appDb.Categories.AddRangeAsync(categories, ct);
            await appDb.SaveChangesAsync(ct);

            return await AuthHelper.SignInAsync(user, "Registered successfully", tokenService, http, ct);
        }
            

        private static string? SanitizeInput(string? input) =>
            string.IsNullOrWhiteSpace(input) ? null : System.Net.WebUtility.HtmlEncode(input.Trim());
    }
}
