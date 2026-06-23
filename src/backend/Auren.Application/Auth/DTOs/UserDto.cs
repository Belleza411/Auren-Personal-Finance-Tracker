using Auren.Application.Profile.DTOs;

namespace Auren.Application.Auth.DTOs
{
	public sealed record UserDto(
         string? Email,
         string? FirstName,
         string? LastName,
         ProfileImageUploadRequest? ProfilePictureUrl,
         string? Currency
    );
}
