using Auren.API.Middleware;

namespace Auren.API.Extensions
{
	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder applicationBuilder)
		{
			return applicationBuilder.UseMiddleware<SecurityHeadersMiddleware>();
		}

        public static IApplicationBuilder UseTokenManagement(this IApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.UseMiddleware<TokenManagementMiddleware>();
        }
    }
}
