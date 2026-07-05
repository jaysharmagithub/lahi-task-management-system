using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Employee;

namespace TaskManagement.Application.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default);
    Task<EmployeeDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default);
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
