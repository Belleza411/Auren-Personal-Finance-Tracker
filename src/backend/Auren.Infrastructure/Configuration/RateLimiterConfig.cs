using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.RateLimiting;

namespace Auren.Infrastructure.Configuration
{
    public static class RateLimiterConfig
    {
        public static void AddRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsync(
                        """{"error":"rate_limit_exceeded","message":"Too many requests. Please retry shortly."}""",
                        token);
                };

                options.AddSlidingWindowLimiter("read", opt =>
                {
                    opt.PermitLimit = 60;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.SegmentsPerWindow = 6;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                options.AddSlidingWindowLimiter("write", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.SegmentsPerWindow = 6;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 0;
                });

                options.AddConcurrencyLimiter("sensitive", opt =>
                {
                    opt.PermitLimit = 1; 
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 10;
                });

                options.AddSlidingWindowLimiter("auth", opt =>
                {
                    opt.PermitLimit = 5;            
                    opt.Window = TimeSpan.FromMinutes(15); 
                    opt.SegmentsPerWindow = 3;     
                    opt.QueueLimit = 0;              
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var endpint = context.GetEndpoint();
                    var hasNamedPolicy = endpint?.Metadata
                        .GetMetadata<EnableRateLimitingAttribute>() != null;

                    if (hasNamedPolicy)
                        return RateLimitPartition.GetNoLimiter<string>("no-limit");

                    var userId = context.User.FindFirst("UserId")?.Value;

                    if (string.IsNullOrEmpty(userId))
                        userId = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: userId,
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
            });
        }
    }
}
