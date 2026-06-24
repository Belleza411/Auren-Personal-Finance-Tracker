using Auren.Application.Features.Profile.DTOs;

namespace Auren.Application.Features.Auth.DTOs
{
	public sealed record UserDto(
         string? Email,
         string? FirstName,
         string? LastName,
         ProfileImageUploadRequest? ProfilePictureUrl,
         string? Currency
    );
}
