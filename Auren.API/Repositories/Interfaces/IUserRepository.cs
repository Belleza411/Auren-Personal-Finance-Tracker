using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Auren.API.Repositories.Interfaces
{
	public interface IUserRepository
	{
		Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
		Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}
