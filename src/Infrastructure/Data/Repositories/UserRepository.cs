using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Report;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Infrastructure.Data.Repositories;

public sealed class UserRepository(ApplicationDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        await DbSet.AnyAsync(u => u.Email == email, ct);

    public async Task<int> CountEmployeesAsync(CancellationToken ct = default) =>
        await DbSet.CountAsync(u => u.Role == UserRole.Employee, ct);

    public async Task<IReadOnlyList<EmployeeTaskReportRow>> GetEmployeeTaskSummaryAsync(CancellationToken ct = default) =>
        await DbSet.AsNoTracking()
            .Where(u => u.Role == UserRole.Employee)
            .Select(u => new EmployeeTaskReportRow(
                u.FullName,
                u.Department ?? string.Empty,
                u.AssignedTasks.Count(t => !t.IsDeleted),
                u.AssignedTasks.Count(t => t.Status == TaskStatus.Completed),
                u.AssignedTasks.Count(t => t.Status == TaskStatus.Pending),
                u.AssignedTasks.Count(t => t.Status == TaskStatus.Overdue)))
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetEmployeesPagedAsync(
        PaginationQuery query, CancellationToken ct = default)
    {
        var q = DbSet.AsNoTracking()
            .Where(u => u.Role == UserRole.Employee)
            .Include(u => u.AssignedTasks)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(u => u.FullName.Contains(query.Search) || u.Email.Contains(query.Search)
                || (u.Department != null && u.Department.Contains(query.Search)));

        q = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
        {
            ("email", "desc") => q.OrderByDescending(u => u.Email),
            ("email", _) => q.OrderBy(u => u.Email),
            ("department", "desc") => q.OrderByDescending(u => u.Department),
            ("department", _) => q.OrderBy(u => u.Department),
            (_, "desc") => q.OrderByDescending(u => u.FullName),
            _ => q.OrderBy(u => u.FullName)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<User?> GetWithTasksAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(u => u.AssignedTasks)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
}
