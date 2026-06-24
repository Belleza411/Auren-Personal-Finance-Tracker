using Auren.Application.Common.Interfaces;
using Auren.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auren.Infrastructure.DI
{
    public static class DatabaseRegistration
    {
        public static void AddDatabases(
            this WebApplicationBuilder builder,
            IConfiguration config)
        {
            builder.Services.AddDbContext<AurenDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("AurenDbConnection"), opt =>
                {
                    opt.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                    opt.CommandTimeout(30);
                }));

            builder.Services.AddDbContext<AurenAuthDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("AurenAuthDbConnection"), opt =>
                {
                    opt.EnableRetryOnFailure(
                       maxRetryCount: 3,
                       maxRetryDelay: TimeSpan.FromSeconds(5),
                       errorNumbersToAdd: null
                   );
                    opt.CommandTimeout(15);
                }));

            builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AurenDbContext>());
            builder.Services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AurenAuthDbContext>());
        }
    }
}
