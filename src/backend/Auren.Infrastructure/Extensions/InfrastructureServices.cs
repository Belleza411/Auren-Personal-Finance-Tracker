using Auren.Application.Interfaces.Repositories;
using Auren.Domain.Entities;
using Auren.Infrastructure.Persistence;
using Auren.Infrastructure.Repositories;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using CloudinaryConfiguration = Auren.Infrastructure.Configuration.CloudinaryConfiguration;

namespace Auren.Infrastructure.Extensions
{
    public static class InfrastructureServices
    {
        public static void AddInfrastructureServices(
            this WebApplicationBuilder builder,
            IConfiguration config)
        {
            builder.Services.AddDbContext<AurenDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("AurenDbConnection")));

            builder.Services.AddDbContext<AurenAuthDbContext>(options => 
                options.UseSqlServer(config.GetConnectionString("AurenAuthDbConnection")));

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
                    options.Cookie.SameSite = SameSiteMode.Lax;
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

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IGoalRepository, GoalRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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


            var cloudinarySection = config.GetSection("Cloudinary");
            builder.Services.Configure<CloudinaryConfiguration>(cloudinarySection);

            var cloudinaryOptions = cloudinarySection.Get<CloudinaryConfiguration>()
            ?? throw new InvalidOperationException("Cloudinary configuration is missing.");

            var cloudName = cloudinaryOptions.CloudName ??
                throw new InvalidOperationException("Cloudinary CloudName is not configured.");
            var apiKey = cloudinaryOptions.ApiKey ??
                throw new InvalidOperationException("Cloudinary ApiKey is not configured.");
            var apiSecret = cloudinaryOptions.ApiSecret ??
                throw new InvalidOperationException("Cloudinary ApiSecret is not configured.");

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true;

            builder.Services.AddSingleton(cloudinary);
        }
    }  

}
