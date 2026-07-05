using System.Net;
using System.Text.Json;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.API.Middleware;

/// <summary>
/// Catches all unhandled exceptions and returns a consistent ProblemDetails JSON response.
/// Prevents stack traces leaking to clients in production.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title, detail) = ex switch
        {
            NotFoundException e => (HttpStatusCode.NotFound, "Resource Not Found", e.Message),
            ConflictException e => (HttpStatusCode.Conflict, "Conflict", e.Message),
            DomainException e => (HttpStatusCode.BadRequest, "Business Rule Violation", e.Message),
            ForbiddenException e => (HttpStatusCode.Forbidden, "Forbidden", e.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", "Authentication is required."),
            ArgumentException e => (HttpStatusCode.BadRequest, "Invalid Argument", e.Message),
            // Never expose internal exception details to clients in production
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", "Please contact support if the problem persists.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
