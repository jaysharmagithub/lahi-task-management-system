using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Identity;
using TaskManagement.Infrastructure.Services;

namespace TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddHostedService<DueSoonNotificationJob>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }
}
