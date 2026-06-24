using Auren.Application.Common.Interfaces;
using Auren.Application.Extensions;
using Auren.Infrastructure.Identity;
using Auren.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Auren.Infrastructure.DI
{
    public static class ServicesRegistration
    {
        public static void AddScopedServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IIdentityService, IdentityService>();

            builder.Services.Scan(scan => scan
                .FromAssemblies(
                    typeof(ApplicationService).Assembly,
                    typeof(InfrastructureServices).Assembly)
                .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Handler")))
                .AsSelf()
                .WithScopedLifetime());
        }
    }
}
