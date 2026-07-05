using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

/// <summary>In-app notification record.</summary>
public class Notification : BaseEntity
{
    public required string Message { get; set; }
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? TaskId { get; set; }
    public TaskItem? Task { get; set; }
}
