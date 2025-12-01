using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Auren.Infrastructure.Repositories;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        public static IServiceCollection AddCloudinary(this IServiceCollection services, IConfiguration configuration)
        {
            var cloudinarySection = configuration.GetSection("Cloudinary");
            services.Configure<CloudinaryConfig>(cloudinarySection);

            var cloudinaryOptions = cloudinarySection.Get<CloudinaryConfig>()
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
