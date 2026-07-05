using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Interfaces;

public interface IEmailService
{
    Task SendTaskAssignedAsync(string toEmail, string toName, string taskTitle, CancellationToken ct = default);
    Task SendTaskDueSoonAsync(string toEmail, string toName, string taskTitle, DateTime dueDate, CancellationToken ct = default);
    Task SendTaskCompletedAsync(string toEmail, string toName, string taskTitle, CancellationToken ct = default);
}
