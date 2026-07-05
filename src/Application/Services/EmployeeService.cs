using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Employee;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Application.Services;

/// <summary>Admin-only employee CRUD operations.</summary>
public sealed class EmployeeService(
    IUnitOfWork uow,
    IMapper mapper,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    public async Task<PagedResult<EmployeeDto>> GetAllAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var (items, total) = await uow.Users.GetEmployeesPagedAsync(query, ct);
        return new PagedResult<EmployeeDto>(mapper.Map<IReadOnlyList<EmployeeDto>>(items), total, query.Page, query.PageSize);
    }

    public async Task<EmployeeDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await uow.Users.GetWithTasksAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);
        return mapper.Map<EmployeeDto>(user);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken ct = default)
    {
        if (await uow.Users.EmailExistsAsync(request.Email, ct))
            throw new ConflictException($"Email '{request.Email}' is already in use.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Employee,
            Department = request.Department,
            Designation = request.Designation
        };

        await uow.Users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Employee {Id} created", user.Id);
        return mapper.Map<EmployeeDto>(user);
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);

        if (!user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)
            && await uow.Users.EmailExistsAsync(request.Email, ct))
            throw new ConflictException($"Email '{request.Email}' is already in use.");

        user.FullName = request.FullName;
        user.Email = request.Email.ToLowerInvariant();
        user.Department = request.Department;
        user.Designation = request.Designation;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        uow.Users.Update(user);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<EmployeeDto>(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        uow.Users.Update(user);
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Employee {Id} soft-deleted", id);
    }
}
