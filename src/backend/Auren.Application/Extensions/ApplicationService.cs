using Auren.Application.Interfaces.Services;
using Auren.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Auren.Application.Extensions
{
	public static class ApplicationService
	{
        public static void AddApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddValidatorsFromAssembly(typeof(ApplicationService).Assembly);
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IGoalService, GoalService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<IProfileService, ProfileService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
        }
    }
}
