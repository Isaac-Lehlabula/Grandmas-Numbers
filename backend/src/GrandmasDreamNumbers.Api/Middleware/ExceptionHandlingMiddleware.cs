using System.Net;
using FluentValidation;
using GrandmasDreamNumbers.Application.Common.Exceptions;

namespace GrandmasDreamNumbers.Api.Middleware;

/// <summary>
/// Maps Application-layer exceptions to HTTP responses in one place so
/// controllers stay thin. Never includes exception details in the
/// response body - only in the server-side log.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Validation failed", string.Join(" ", ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Not found", ex.Message);
        }
        catch (ForbiddenAccessException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Forbidden, "Forbidden", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred", "Please try again later.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsJsonAsync(new
        {
            title,
            status = (int)statusCode,
            detail,
        });
    }
}
