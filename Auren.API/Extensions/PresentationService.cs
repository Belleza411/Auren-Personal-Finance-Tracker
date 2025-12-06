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

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var userId = context.User.FindFirst("UserId")?.Value;

                    if (string.IsNullOrEmpty(userId))
                        userId = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userId,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                // Strong password policy
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 4;

                // Account lockout
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<AurenAuthDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "Auren.Session";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/auth/login";
                    options.LogoutPath = "/auth/logout";
                    options.AccessDeniedPath = "/auth/access-denied";

                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };

                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenRepository>();
                        await tokenService.ValidateRefreshTokenAsync(context);
                    };

                    options.Events.OnSignedIn = async context =>
                    {
                        var refreshCookieOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(14),
                            Path = "/",
                            IsEssential = true
                        };

                        context.Response.Cookies.Append("Auren.Session.Refresh", "valid", refreshCookieOptions);

                        await Task.CompletedTask;
                    };

                    options.Events.OnSigningOut = async context =>
                    {
                        var expiredOptions = new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTimeOffset.UtcNow.AddDays(-1), 
                            Path = "/"
                        };

                        context.Response.Cookies.Append("Auren.Session.Refresh", "", expiredOptions);

                        await Task.CompletedTask;
                    };
                });
        }
    }
}
