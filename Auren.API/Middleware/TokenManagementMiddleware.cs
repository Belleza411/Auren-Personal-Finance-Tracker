using Auren.API.Helpers;

namespace Auren.API.Middleware
{
	public class TokenManagementMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<TokenManagementMiddleware> _logger;
		private readonly CookieHelper _cookieHelper;

        public TokenManagementMiddleware(RequestDelegate next, ILogger<TokenManagementMiddleware> logger, CookieHelper cookieHelper)
		{
			_next = next;
			_logger = logger;
			_cookieHelper = cookieHelper;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// Handle Login
			if(context.Request.Path == "/auth/login"
				&& context.Request.Method == "POST"
				&& context.User.Identity?.IsAuthenticated == true)
			{
				await _cookieHelper.SetSecureCookiesAsync(context);
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
				await _cookieHelper.ClearSecureCookiesAsync(context);
            }

			await _next(context);
        }
	}
}
