using Auren.API.Data;
using Auren.API.Extensions;
using Auren.API.Helpers;
using Auren.API.Models.Domain;
using Auren.API.Repositories.Implementations;
using Auren.API.Repositories.Interfaces;
using Auren.API.Services.Implementations;
using Auren.API.Services.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.DateFormatString = "MMMM dd, yyyy";
    options.SerializerSettings.DateParseHandling = Newtonsoft.Json.DateParseHandling.DateTime;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Auren.Cookie", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "Auren.Session",
        Description = "Cookie-based authentication using Auren.Session cookie."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Auren.Cookie"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AurenAuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AurenAuthDbConnection"));
});
builder.Services.AddDbContext<AurenDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AurenDbConnection"));
});

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

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
  //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "Auren.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // Short access token lifetime
        options.SlidingExpiration = false;
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/access-denied";

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
                Expires = DateTimeOffset.UtcNow.AddDays(14), // Longer lifetime for refresh token
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
                Expires = DateTimeOffset.UtcNow.AddDays(-1), // Expire immediately
                Path = "/"
            };

            context.Response.Cookies.Append("Auren.Session.Refresh", "", expiredOptions);

            await Task.CompletedTask;
        };
    });

builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();

builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection("FileUpload"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSecurityHeaders();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var authDbContext = scope.ServiceProvider.GetRequiredService<AurenAuthDbContext>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AurenDbContext>();

    authDbContext.Database.EnsureCreated();
    dbContext.Database.EnsureCreated();
}

app.MapControllers();

app.Run();
