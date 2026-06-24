using Auren.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Auren.Infrastructure.DI
{
    public static class InfrastructureServices
    {
        public static void AddInfrastructureServices(
            this WebApplicationBuilder builder,
            IConfiguration config)
        {
            builder.AddDatabases(config);
            builder.AddAuthServices();
            builder.AddCloudinary(config);
            builder.AddScopedServices();
            builder.Services.AddRateLimiting();
        }
    }  

}
