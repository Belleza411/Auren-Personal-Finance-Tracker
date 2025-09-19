using System.Security.Claims;

namespace Auren.API.Helpers
{
	public class CookieHelper
	{
		private readonly ILogger<CookieHelper> _logger;

		public CookieHelper(ILogger<CookieHelper> logger)
		{
			_logger = logger;
		}

		public async Task SetSecureCookiesAsync(HttpContext context)
		{
			try
			{
				var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (!string.IsNullOrEmpty(userId))
				{
					// Set cookies
					var cookieOptions = new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Lax,
						Expires = DateTimeOffset.UtcNow.AddMinutes(10),
						Path = "/"
					};

					// Set a simple session cookie but not an actual token
					context.Response.Cookies.Append("Auren.Session", "active", cookieOptions);

					// Set refresh token cookie with longer lifetime
					var refreshTokenOptions = new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Lax,
						Expires = DateTimeOffset.UtcNow.AddDays(14), // Longer lifetime for refresh token
						Path = "/",
						IsEssential = true
					};

					context.Response.Cookies.Append("Auren.Session.Refresh", "valid", refreshTokenOptions);

					_logger.LogInformation("Set secure cookies for user {UserId}", userId);
				}
			}
			catch(Exception ex)
			{
                _logger.LogError(ex, "Error setting secure cookies");
            }

			await Task.CompletedTask;
        }

		public async Task ClearSecureCookiesAsync(HttpContext context)
		{
			try
			{
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1), // Expire immediately
                    Path = "/"
                };

                context.Response.Cookies.Append("Auren.Session", "", cookieOptions);
                context.Response.Cookies.Append("Auren.Session.Refresh", "", cookieOptions);

                // Also clear the main auth cookie
                context.Response.Cookies.Append("Auren", "", cookieOptions);

                _logger.LogInformation("Cleared secure cookies");
            }
			catch(Exception ex)
			{
                _logger.LogError(ex, "Error clearing secure cookies");
            }

			await Task.CompletedTask;
        }
	}
}
