
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Domain.Entities;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IUserRepository
	{
        Task<ApplicationUser?> GetUserById(Guid userId, CancellationToken cancellationToken);

        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
		Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}
