using Auren.Application.Common.Result;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auren.Application.Interfaces.Services
{
	public interface IUserService
	{
        Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        Task<Result<bool>> LogoutAsync(CancellationToken cancellationToken);
    }
}
