using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Report;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetEmployeesPagedAsync(PaginationQuery query, CancellationToken ct = default);
    Task<User?> GetWithTasksAsync(Guid id, CancellationToken ct = default);
    Task<int> CountEmployeesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeTaskReportRow>> GetEmployeeTaskSummaryAsync(CancellationToken ct = default);
}
