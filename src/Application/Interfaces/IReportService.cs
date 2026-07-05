using TaskManagement.Application.DTOs.Report;

namespace TaskManagement.Application.Interfaces;

public interface IReportService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskReportRow>> GetCompletedTasksReportAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TaskReportRow>> GetPendingTasksReportAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeTaskReportRow>> GetEmployeeWiseReportAsync(CancellationToken ct = default);
    Task<byte[]> ExportToExcelAsync(string reportType, CancellationToken ct = default);
    Task<byte[]> ExportToCsvAsync(string reportType, CancellationToken ct = default);
}
