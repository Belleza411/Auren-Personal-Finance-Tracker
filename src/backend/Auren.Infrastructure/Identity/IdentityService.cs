using Auren.Application.Common.Interfaces;
using Auren.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Auren.Infrastructure.Identity
{
    public class IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager) : IIdentityService
    {
        public Task<ApplicationUser?> FindByEmailAsync(string email)
            => userManager.FindByEmailAsync(email);

        public Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
            => userManager.CreateAsync(user, password);

        public Task<IdentityResult> UpdateAsync(ApplicationUser user)
            => userManager.UpdateAsync(user);

        public Task<SignInResult> CheckPasswordAsync(ApplicationUser user, string password)
            => signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user)
            => userManager.GetLockoutEndDateAsync(user);

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user)
            => userManager.GetAccessFailedCountAsync(user);

        public Task<IdentityResult> ChangePasswordAsync(
            ApplicationUser user,
            string currentPassword,
            string newPassword)
                => userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        public Task<ApplicationUser?> FindByIdAsync(Guid userId)
            => userManager.FindByIdAsync(userId.ToString());

        public Task<IdentityResult> DeleteAsync(ApplicationUser user)
            => userManager.DeleteAsync(user);
    }
}
