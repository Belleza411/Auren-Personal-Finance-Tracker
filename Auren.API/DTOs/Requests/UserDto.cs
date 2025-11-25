namespace Auren.API.DTOs.Requests
{
	public sealed record UserDto(
         string? Email,
         string? FirstName,
         string? LastName,
         ProfileImageUploadRequest? ProfilePictureUrl,
         string? Currency
    );
}
