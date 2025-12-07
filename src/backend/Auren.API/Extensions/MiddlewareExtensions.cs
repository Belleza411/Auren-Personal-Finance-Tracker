using Auren.API.Middleware;
using Microsoft.AspNetCore.Builder;

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
