using Auren.Application.Common.Result;
using Auren.Application.Features.Auth.DTOs;
using Auren.Domain.Entities;

namespace Auren.Application.Common.Interfaces
{
    public interface IAuthHelper
    {
        Task<Result<AuthResponse>> SignInAsync( 
            ApplicationUser user,
            string message,
            CancellationToken ct);
    }
}
