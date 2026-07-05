using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Task;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Data.Repositories;

public sealed class TaskRepository(ApplicationDbContext context)
    : Repository<TaskItem>(context), ITaskRepository
{
    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(
        TaskFilterQuery query, Guid? userId, CancellationToken ct = default)
    {
        var q = DbSet.AsNoTracking()
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .AsQueryable();

        if (userId.HasValue)
            q = q.Where(t => t.AssignedToId == userId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(t => t.Title.Contains(query.Search) || (t.Description != null && t.Description.Contains(query.Search)));

        if (query.Status.HasValue)
            q = q.Where(t => t.Status == query.Status.Value);

        if (query.Priority.HasValue)
            q = q.Where(t => t.Priority == query.Priority.Value);

        if (query.AssignedToId.HasValue)
            q = q.Where(t => t.AssignedToId == query.AssignedToId.Value);

        q = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
        {
            ("duedate", "desc") => q.OrderByDescending(t => t.DueDate),
            ("duedate", _) => q.OrderBy(t => t.DueDate),
            ("priority", "desc") => q.OrderByDescending(t => t.Priority),
            ("priority", _) => q.OrderBy(t => t.Priority),
            ("status", "desc") => q.OrderByDescending(t => t.Status),
            ("status", _) => q.OrderBy(t => t.Status),
            (_, "desc") => q.OrderByDescending(t => t.CreatedAt),
            _ => q.OrderByDescending(t => t.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<TaskItem?> GetWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<TaskItem>> GetDueSoonAsync(DateTime threshold, CancellationToken ct = default) =>
        await DbSet
            .Include(t => t.AssignedTo)
            .Where(t => t.Status != TaskStatus.Completed
                && !t.IsDueSoonNotificationSent
                && t.DueDate <= threshold
                && t.DueDate >= DateTime.UtcNow)
            .ToListAsync(ct);

    public async Task<int> CountByStatusAsync(TaskStatus status, Guid? userId = null, CancellationToken ct = default)
    {
        var q = DbSet.AsNoTracking().Where(t => t.Status == status);
        if (userId.HasValue) q = q.Where(t => t.AssignedToId == userId.Value);
        return await q.CountAsync(ct);
    }
}