using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Services;

/// <summary>Sends transactional emails via SMTP using MailKit.</summary>
public sealed class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    private readonly string _host = configuration["Email:Host"] ?? "localhost";
    private readonly int _port = int.Parse(configuration["Email:Port"] ?? "587");
    private readonly string _username = configuration["Email:Username"] ?? string.Empty;
    private readonly string _password = configuration["Email:Password"] ?? string.Empty;
    private readonly string _fromEmail = configuration["Email:From"] ?? "noreply@taskmanagement.com";
    private readonly string _fromName = configuration["Email:FromName"] ?? "Task Management";

    public Task SendTaskAssignedAsync(string toEmail, string toName, string taskTitle, CancellationToken ct = default) =>
        SendAsync(toEmail, toName, "New Task Assigned",
            $"<p>Hi {toName},</p><p>You have been assigned a new task: <strong>{taskTitle}</strong>.</p>", ct);

    public Task SendTaskDueSoonAsync(string toEmail, string toName, string taskTitle, DateTime dueDate, CancellationToken ct = default) =>
        SendAsync(toEmail, toName, "Task Due Soon",
            $"<p>Hi {toName},</p><p>Task <strong>{taskTitle}</strong> is due on {dueDate:yyyy-MM-dd HH:mm} UTC.</p>", ct);

    public Task SendTaskCompletedAsync(string toEmail, string toName, string taskTitle, CancellationToken ct = default) =>
        SendAsync(toEmail, toName, "Task Completed",
            $"<p>Hi {toName},</p><p>Task <strong>{taskTitle}</strong> has been marked as complete.</p>", ct);

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTlsWhenAvailable, ct);
            if (!string.IsNullOrEmpty(_username))
                await client.AuthenticateAsync(_username, _password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email} with subject {Subject}", toEmail, subject);
        }
    }
}
