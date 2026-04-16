using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Auren.API.Middleware
{
	public class GlobalExceptionMiddleware(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionMiddleware> logger) : IExceptionHandler
	{
		public async ValueTask<bool> TryHandleAsync(
			HttpContext context,
			Exception exception,
			CancellationToken cancellationToken)
		{
            if(exception is OperationCanceledException)
            {
                logger.LogInformation("Request was cancelled (client disconnected or hot reload).");

                context.Response.StatusCode = 499;

                return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    Exception = exception,
                    ProblemDetails = new ProblemDetails
                    {
                        Type = nameof(OperationCanceledException),
                        Title = "Request Cancelled",
                        Detail = "The request was cancelled by the client.",
                        Status = StatusCodes.Status499ClientClosedRequest,
                        Instance = context.Request.GetDisplayUrl()
                    }
                });
            }

            logger.LogError(exception, "Something went wrong");

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Type = exception.GetType().Name,
                    Title = "An error occured",
                    Detail = exception.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.GetDisplayUrl()
                }
            });
		}
	}
}
