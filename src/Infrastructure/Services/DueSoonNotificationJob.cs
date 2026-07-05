using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Services;

/// <summary>Background service that checks for tasks due within 24 hours and sends notifications.</summary>
public sealed class DueSoonNotificationJob(IServiceScopeFactory scopeFactory, ILogger<DueSoonNotificationJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred executing DueSoonNotificationJob");
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope(); // Corrected for IAsyncDisposable
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var threshold = DateTime.UtcNow.AddHours(24);
        var tasks = await uow.Tasks.GetDueSoonAsync(threshold, ct);

        foreach (var task in tasks)
        {
            await uow.Notifications.AddAsync(new Notification
            {
                Message = $"Task '{task.Title}' is due within 24 hours.",
                Type = NotificationType.TaskDueSoon,
                UserId = task.AssignedToId,
                TaskId = task.Id
            }, ct);

            task.IsDueSoonNotificationSent = true;
            uow.Tasks.Update(task);

            await emailService.SendTaskDueSoonAsync(
                task.AssignedTo.Email, task.AssignedTo.FullName, task.Title, task.DueDate, ct);
        }

        if (tasks.Count > 0)
        {
            await uow.SaveChangesAsync(ct);
            logger.LogInformation("Sent due-soon notifications for {Count} tasks", tasks.Count);
        }
    }
}
