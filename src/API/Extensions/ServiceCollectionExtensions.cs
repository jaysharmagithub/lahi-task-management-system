using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.OpenApi.Models;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Task Management API",
                Version = "v1",
                Description = "Employee Task Management System — ASP.NET Core 8 Web API"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}

public static class WebApplicationExtensions
{
    public static async Task MigrateDbAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Verifying database state...");

            // RelationalDatabaseCreator allows us to check for tables specifically
            var databaseCreator = db.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            if (databaseCreator != null)
            {
                // Create database if it doesn't exist
                if (!await databaseCreator.ExistsAsync()) await databaseCreator.CreateAsync();

                // Create tables ONLY if they are missing. This preserves existing data.
                if (!await databaseCreator.HasTablesAsync())
                {
                    logger.LogInformation("Tables missing. Creating schema...");
                    await databaseCreator.CreateTablesAsync();
                }
            }

            logger.LogInformation("Database is ready and data is preserved.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
        }
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=()");
            await next();
        });
}
