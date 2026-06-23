using Auren.Application.Profile.DTOs;

namespace Auren.Application.Auth.DTOs
{
	public sealed record RegisterRequest(
		string Email,
		string Password,
		string ConfirmPassword,
		string FirstName,
		string LastName,
		ProfileImageUploadRequest? ProfileImage
    );
}
