using Auren.API.Middleware;

namespace Auren.API.Extensions
{
	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder applicationBuilder)
		{
			return applicationBuilder.UseMiddleware<SecurityHeadersMiddleware>();
		}
    }
}
