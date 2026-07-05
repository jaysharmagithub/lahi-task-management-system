using TaskManagement.Application.DTOs.Task;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(TaskFilterQuery query, Guid? userId, CancellationToken ct = default);
    Task<TaskItem?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetDueSoonAsync(DateTime threshold, CancellationToken ct = default);
    Task<int> CountByStatusAsync(Domain.Enums.TaskStatus status, Guid? userId = null, CancellationToken ct = default);
}
