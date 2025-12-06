using Auren.API.Middleware;
using Newtonsoft.Json.Converters;

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
