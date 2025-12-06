using Auren.API.Middleware;
using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json.Converters;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;

namespace Auren.API.Extensions
{
	public static class PresentationService
	{
		public static void AddPresentationServices(this WebApplicationBuilder builder)
		{
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateFormatString = "MMMM dd, yyyy";
                    options.SerializerSettings.DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTime;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddProblemDetails(configure =>
            {
                configure.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                };
            });

            builder.Services.AddExceptionHandler<GlobalExceptionMiddleware>();
        }
    }
}
