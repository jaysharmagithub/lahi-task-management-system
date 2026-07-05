using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Dashboard stats and report exports (Excel/CSV).
/// Admin dashboard: total employees, tasks, completed, pending, overdue.
/// Employee dashboard: my tasks, completed, pending, overdue.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class ReportsController(IReportService reportService) : BaseController
{
    /// <summary>Admin dashboard summary statistics.</summary>
    [HttpGet("dashboard/admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AdminDashboard(CancellationToken ct) =>
        Ok(await reportService.GetAdminDashboardAsync(ct));

    /// <summary>Employee dashboard summary for the current user.</summary>
    [HttpGet("dashboard/employee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmployeeDashboard(CancellationToken ct) =>
        Ok(await reportService.GetEmployeeDashboardAsync(CurrentUserId, ct));

    /// <summary>Completed tasks report data.</summary>
    [HttpGet("completed")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CompletedTasks(CancellationToken ct) =>
        Ok(await reportService.GetCompletedTasksReportAsync(ct));

    /// <summary>Pending tasks report data.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PendingTasks(CancellationToken ct) =>
        Ok(await reportService.GetPendingTasksReportAsync(ct));

    /// <summary>Employee-wise task summary report.</summary>
    [HttpGet("employee-wise")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EmployeeWise(CancellationToken ct) =>
        Ok(await reportService.GetEmployeeWiseReportAsync(ct));

    /// <summary>Export report to Excel (.xlsx). reportType: completed | pending | employee</summary>
    [HttpGet("export/excel/{reportType}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExcel(string reportType, CancellationToken ct)
    {
        var bytes = await reportService.ExportToExcelAsync(reportType, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{reportType}-report-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    /// <summary>Export report to CSV. reportType: completed | pending | employee</summary>
    [HttpGet("export/csv/{reportType}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCsv(string reportType, CancellationToken ct)
    {
        var bytes = await reportService.ExportToCsvAsync(reportType, ct);
        return File(bytes, "text/csv", $"{reportType}-report-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
