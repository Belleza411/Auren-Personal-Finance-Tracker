using Auren.Application.Common;
using Auren.Application.DTOs.Requests;
using Auren.Application.DTOs.Responses;
using Auren.Application.Interfaces.Repositories;
using Auren.Application.Interfaces.Services;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auren.Infrastructure.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly AurenAuthDbContext _dbContext;

		public UserRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AurenAuthDbContext dbContext)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_dbContext = dbContext;
		}

		public async Task<ApplicationUser?> GetUserById(Guid userId, CancellationToken cancellationToken)
			=> await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

		public async Task<ApplicationUser?> FindEmailAsync(string email)
			=> await _userManager.FindByEmailAsync(email);

		public async Task<SignInResult> CheckPasswordAsync(ApplicationUser user, string password)
			=> await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

		public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
			=> await _userManager.CreateAsync(user, password);

		public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
			=> await _userManager.UpdateAsync(user);

		public async Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user)
			=> await _userManager.GetLockoutEndDateAsync(user);

		public async Task<int> GetAccessFailedCountAsync(ApplicationUser user)
			=> await _userManager.GetAccessFailedCountAsync(user);
    }
}
