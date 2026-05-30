using DegeneraTube.API.Middleware;

namespace DegeneraTube.API.Extensions;

public static class AppExtensions
{
    public static WebApplication UseAppMiddleware(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();

        return app;
    }

    public static WebApplication UseAppSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}