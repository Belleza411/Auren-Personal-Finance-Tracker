using Auren.Application.Interfaces.Repositories;
using Auren.Infrastructure.Persistence;
using Auren.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auren.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            services.AddDbContext<AurenDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("AurenDbConnection")));

            services.AddDbContext<AurenAuthDbContext>(options => 
                options.UseSqlServer(config.GetConnectionString("AurenAuthDbConnection")));
          
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IGoalRepository, GoalRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();

            return services;
        }
    }  

}
