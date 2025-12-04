using Auren.Application.Interfaces.Repositories;
using Auren.Infrastructure.Configuration;
using Auren.Infrastructure.Persistence;
using Auren.Infrastructure.Repositories;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CloudinaryConfiguration = Auren.Infrastructure.Configuration.CloudinaryConfiguration;

namespace Auren.Infrastructure.Extensions
{
    public static class InfrastructureServices
    {
        public static IServiceCollection AddInfrastructureServices(
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

            var cloudinarySection = config.GetSection("Cloudinary");
            services.Configure<CloudinaryConfiguration>(cloudinarySection);

            var cloudinaryOptions = cloudinarySection.Get<CloudinaryConfiguration>()
            ?? throw new InvalidOperationException("Cloudinary configuration is missing.");

            var cloudName = cloudinaryOptions.CloudName ??
                throw new InvalidOperationException("Cloudinary CloudName is not configured.");
            var apiKey = cloudinaryOptions.ApiKey ??
                throw new InvalidOperationException("Cloudinary ApiKey is not configured.");
            var apiSecret = cloudinaryOptions.ApiSecret ??
                throw new InvalidOperationException("Cloudinary ApiSecret is not configured.");

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;

            services.AddSingleton(cloudinary);

            return services;
        }
    }  

}
