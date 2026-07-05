namespace TaskManagement.Application.DTOs.Report;

public record AdminDashboardDto(
    int TotalEmployees,
    int TotalTasks,
    int CompletedTasks,
    int PendingTasks,
    int OverdueTasks);

public record EmployeeDashboardDto(
    int MyTasks,
    int CompletedTasks,
    int PendingTasks,
    int OverdueTasks);

public record TaskReportRow(
    string Title,
    string AssignedTo,
    string Priority,
    string Status,
    DateTime StartDate,
    DateTime DueDate);

public record EmployeeTaskReportRow(
    string EmployeeName,
    string Department,
    int TotalTasks,
    int Completed,
    int Pending,
    int Overdue);
