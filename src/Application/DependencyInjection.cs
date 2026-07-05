using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mapping;
using TaskManagement.Application.Services;

namespace TaskManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
