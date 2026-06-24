using Auren.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auren.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateAsync(ApplicationUser user);
        Task<SignInResult> CheckPasswordAsync(ApplicationUser user, string password);
        Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user);
        Task<int> GetAccessFailedCountAsync(ApplicationUser user);
    }
}
