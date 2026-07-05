using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Domain.Entities;

/// <summary>Task assigned to an employee.</summary>
public class TaskItem : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? AttachmentPath { get; set; }
    public string? AttachmentFileName { get; set; }
    public bool IsDueSoonNotificationSent { get; set; }

    public Guid AssignedToId { get; set; }
    public User AssignedTo { get; set; } = null!;

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<Notification> Notifications { get; set; } = [];
}