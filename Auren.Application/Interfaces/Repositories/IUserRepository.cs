
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IUserRepository
	{
		Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
		Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}
