using DegeneraTube.Shared;
using System.Text.Json;

namespace DegeneraTube.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, message) = ex switch
        {
            NotFoundException e    => (e.StatusCode, e.Message),
            UnauthorizedException e => (e.StatusCode, e.Message),
            ForbiddenException e   => (e.StatusCode, e.Message),
            ValidationException e  => (e.StatusCode, e.Message),
            ConflictException e    => (e.StatusCode, e.Message),
            AppException e         => (e.StatusCode, e.Message),
            _                      => (500, "Internal server error.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var body = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(body);
    }
}