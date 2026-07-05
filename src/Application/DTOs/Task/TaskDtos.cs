using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.DTOs.Task;

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? AttachmentFileName { get; set; }
    public Guid AssignedToId { get; set; }
    public string AssignedToName { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime StartDate,
    DateTime DueDate,
    Guid AssignedToId);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime StartDate,
    DateTime DueDate,
    Guid AssignedToId);

public record TaskFilterQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = "asc",
    TaskStatus? Status = null,
    TaskPriority? Priority = null,
    Guid? AssignedToId = null);
