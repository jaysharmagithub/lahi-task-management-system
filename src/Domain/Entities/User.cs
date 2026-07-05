using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

/// <summary>Application user — both Admin and Employee roles.</summary>
public class User : BaseEntity
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TaskItem> AssignedTasks { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
}
