using Auren.API.Helpers;

namespace Auren.API.Middleware
{
	public class TokenManagementMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<TokenManagementMiddleware> _logger;

		public TokenManagementMiddleware(RequestDelegate next, ILogger<TokenManagementMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var cookieHelper = context.RequestServices.GetRequiredService<CookieHelper>();

            // Handle Login
            if (context.Request.Path == "/auth/login"
				&& context.Request.Method == "POST"
				&& context.User.Identity?.IsAuthenticated == true)
			{
				await cookieHelper.SetSecureCookiesAsync(context);
			}

			// Handle Google OAuth Callback
			/*
			if(context.Request.Path.StartsWithSegments("/auth/google-callback")
				&& context.User.Identity?.IsAuthenticated == true)
			{
				await _cookieHelper.SetSecureCookiesAsync(context);
			}
			*/

			// Handler logout
			if (context.Request.Path == "/auth/logout")
			{
				await cookieHelper.ClearSecureCookiesAsync(context);
            }

			await _next(context);
        }
	}
}
