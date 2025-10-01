using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Auren.API.Repositories.Interfaces
{
	public interface IProfileRepository
	{
		Task<UserResponse?> GetUserProfile(Guid userId, CancellationToken cancellationToken);
		Task<UserResponse?> UpdateUserProfile(Guid userId, UserDto userDto, CancellationToken cancellationToken);

    }
}
