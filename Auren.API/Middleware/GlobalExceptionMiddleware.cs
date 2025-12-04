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
                    Instance = context.Request.Path
                }
            });
		}
	}
}
