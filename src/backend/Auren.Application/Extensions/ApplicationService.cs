using FluentValidation;
using Microsoft.AspNetCore.Builder;

namespace Auren.Application.Extensions
{
    public static class ApplicationService
    {
        public static void AddApplicationServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddValidatorsFromAssembly(typeof(ApplicationService).Assembly);
        }
    }
}
