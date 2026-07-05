using TaskManagement.Application.DTOs.Report;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Services;

public sealed class ReportService(IUnitOfWork uow, IExportService exportService) : IReportService
{
    private static class ReportType
    {
        public const string Completed = "completed";
        public const string Pending = "pending";
        public const string Employee = "employee";
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var totalEmployees = await uow.Users.CountEmployeesAsync(ct);
        var completed = await uow.Tasks.CountByStatusAsync(TaskStatus.Completed, null, ct);
        var pending = await uow.Tasks.CountByStatusAsync(TaskStatus.Pending, null, ct);
        var overdue = await uow.Tasks.CountByStatusAsync(TaskStatus.Overdue, null, ct);
        var inProgress = await uow.Tasks.CountByStatusAsync(TaskStatus.InProgress, null, ct);
        return new AdminDashboardDto(totalEmployees, completed + pending + overdue + inProgress, completed, pending, overdue);
    }

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(Guid userId, CancellationToken ct = default)
    {
        var completed = await uow.Tasks.CountByStatusAsync(TaskStatus.Completed, userId, ct);
        var pending = await uow.Tasks.CountByStatusAsync(TaskStatus.Pending, userId, ct);
        var overdue = await uow.Tasks.CountByStatusAsync(TaskStatus.Overdue, userId, ct);
        var inProgress = await uow.Tasks.CountByStatusAsync(TaskStatus.InProgress, userId, ct);
        return new EmployeeDashboardDto(completed + pending + overdue + inProgress, completed, pending, overdue);
    }

    public async Task<IReadOnlyList<TaskReportRow>> GetCompletedTasksReportAsync(CancellationToken ct = default) =>
        await GetTaskReportRowsAsync(TaskStatus.Completed, ct);

    public async Task<IReadOnlyList<TaskReportRow>> GetPendingTasksReportAsync(CancellationToken ct = default) =>
        await GetTaskReportRowsAsync(TaskStatus.Pending, ct);

    public async Task<IReadOnlyList<EmployeeTaskReportRow>> GetEmployeeWiseReportAsync(CancellationToken ct = default) =>
        await uow.Users.GetEmployeeTaskSummaryAsync(ct);

    public async Task<byte[]> ExportToExcelAsync(string reportType, CancellationToken ct = default)
    {
        return reportType.ToLowerInvariant() switch
        {
            ReportType.Completed => await exportService.ExportToExcelAsync(
                await GetCompletedTasksReportAsync(ct), "Completed Tasks", ct),
            ReportType.Pending => await exportService.ExportToExcelAsync(
                await GetPendingTasksReportAsync(ct), "Pending Tasks", ct),
            ReportType.Employee => await exportService.ExportToExcelAsync(
                await GetEmployeeWiseReportAsync(ct), "Employee Summary", ct),
            _ => throw new ArgumentException($"Unknown report type: {reportType}")
        };
    }

    public async Task<byte[]> ExportToCsvAsync(string reportType, CancellationToken ct = default)
    {
        return reportType.ToLowerInvariant() switch
        {
            ReportType.Completed => await exportService.ExportToCsvAsync(
                await GetCompletedTasksReportAsync(ct), ct),
            ReportType.Pending => await exportService.ExportToCsvAsync(
                await GetPendingTasksReportAsync(ct), ct),
            ReportType.Employee => await exportService.ExportToCsvAsync(
                await GetEmployeeWiseReportAsync(ct), ct),
            _ => throw new ArgumentException($"Unknown report type: {reportType}")
        };
    }

    private async Task<IReadOnlyList<TaskReportRow>> GetTaskReportRowsAsync(TaskStatus status, CancellationToken ct)
    {
        var query = new DTOs.Task.TaskFilterQuery(Page: 1, PageSize: int.MaxValue, Status: status);
        var (items, _) = await uow.Tasks.GetPagedAsync(query, null, ct);
        return items.Select(t => new TaskReportRow(
            t.Title, t.AssignedTo.FullName, t.Priority.ToString(), t.Status.ToString(), t.StartDate, t.DueDate))
            .ToList();
    }
}
