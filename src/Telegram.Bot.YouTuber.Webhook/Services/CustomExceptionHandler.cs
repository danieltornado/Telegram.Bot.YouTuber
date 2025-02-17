using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Telegram.Bot.YouTuber.Webhook.Services;

/// <summary>
/// https://www.milanjovanovic.tech/blog/problem-details-for-aspnetcore-apis
/// </summary>
/// <param name="problemDetailsService"></param>
public sealed class CustomExceptionHandler(IProblemDetailsService problemDetailsService, IHostEnvironment hostEnvironment) : IExceptionHandler
{
    #region Implementation of IExceptionHandler

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred",
            Type = "Internal Server Error",
            Detail = "Internal Server Error"
        };

        if (hostEnvironment.IsProduction() is false)
        {
            problemDetails.Type = exception.GetType().Name;
            problemDetails.Detail = exception.ToString();
        }

        // using the IProblemDetailsService gives an easy way to customize all Problem Details responses
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    #endregion
}