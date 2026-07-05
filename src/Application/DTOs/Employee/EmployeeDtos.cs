namespace TaskManagement.Application.DTOs.Employee;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public bool IsActive { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
}

public class EmployeeLookupDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
}

public record CreateEmployeeRequest(
    string FullName,
    string Email,
    string Password,
    string? Department,
    string? Designation);

public record UpdateEmployeeRequest(
    string FullName,
    string Email,
    string? Department,
    string? Designation,
    bool IsActive);
