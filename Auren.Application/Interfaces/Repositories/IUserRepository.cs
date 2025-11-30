
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Auren.Application.Interfaces.Repositories
{
	public interface IUserRepository
	{
        Task<ApplicationUser?> GetUserById(Guid userId, CancellationToken cancellationToken);
        Task<ApplicationUser?> FindEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<SignInResult> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user);
        Task<int> GetAccessFailedCountAsync(ApplicationUser user);
    }
}
