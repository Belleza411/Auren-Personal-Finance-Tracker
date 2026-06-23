using Auren.Application.Features.Profile.DTOs;

namespace Auren.Application.Features.Auth.DTOs
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
