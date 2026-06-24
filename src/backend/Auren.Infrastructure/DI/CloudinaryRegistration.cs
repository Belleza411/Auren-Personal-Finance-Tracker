using Auren.Infrastructure.Common.Interfaces;
using Auren.Infrastructure.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CloudinaryConfiguration = Auren.Infrastructure.Configuration.CloudinaryConfiguration;

namespace Auren.Infrastructure.DI
{
    public static class CloudinaryRegistration
    {
        public static void AddCloudinary(
            this WebApplicationBuilder builder,
            IConfiguration config)
        {
            var section = config.GetSection("Cloudinary");
            builder.Services.Configure<CloudinaryConfiguration>(section);

            var options = section.Get<CloudinaryConfiguration>()
                ?? throw new InvalidOperationException("Cloudinary configuration is missing.");

            var cloudinary = new Cloudinary(new Account(
                options.CloudName ?? throw new InvalidOperationException("Cloudinary CloudName is missing."),
                options.ApiKey ?? throw new InvalidOperationException("Cloudinary ApiKey is missing."),
                options.ApiSecret ?? throw new InvalidOperationException("Cloudinary ApiSecret is missing.")
            ));

            cloudinary.Api.Secure = true;
            builder.Services.AddSingleton(cloudinary);
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
        }
    }
}
