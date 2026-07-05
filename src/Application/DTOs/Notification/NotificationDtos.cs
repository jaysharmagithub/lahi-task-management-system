using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Notification;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public Guid? TaskId { get; set; }
    public DateTime CreatedAt { get; set; }
}
