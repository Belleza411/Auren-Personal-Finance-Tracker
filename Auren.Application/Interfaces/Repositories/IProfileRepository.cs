using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.DTOs.Responses.User;
using Auren.Domain.Entities;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IProfileRepository
	{
		Task<UserResponse?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken);
		Task<UserResponse?> UpdateUserProfileAsync(Guid userId, ApplicationUser user, CancellationToken cancellationToken);
		Task<ProfileImageUploadResponse> UploadProfileImageAsync(ProfileImageUploadRequest request, CancellationToken cancellationToken);
    }
}
