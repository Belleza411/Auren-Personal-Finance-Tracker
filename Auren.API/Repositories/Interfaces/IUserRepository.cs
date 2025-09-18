using Auren.API.DTOs.Requests;
using Auren.API.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Auren.API.Repositories.Interfaces
{
	public interface IUserRepository
	{
		Task<AuthResponse> RegisterAsync(RegisterRequest request);
		Task<AuthResponse> LoginAsync(LoginRequest request);
        bool ValidateInput(object input, out List<string> errors);
    }
}
