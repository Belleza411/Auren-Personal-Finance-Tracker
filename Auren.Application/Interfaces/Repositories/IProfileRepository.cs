using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IProfileRepository
	{
		Task<UserResponse?> GetUserProfile(Guid userId, CancellationToken cancellationToken);
		Task<UserResponse?> UpdateUserProfile(Guid userId, UserDto userDto, CancellationToken cancellationToken);
		Task<ProfileImageUploadResponse> UploadProfileImageAsync(ProfileImageUploadRequest request, CancellationToken cancellationToken);
    }
}
