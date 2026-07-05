using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Employee;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.API.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class EmployeesController(IEmployeeService employeeService, IMapper mapper, IUnitOfWork uow) : BaseController
{
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLookup(CancellationToken ct)
    {
        // Direct repo call to avoid heavy Service logic that expects Includes
        var users = await uow.Users.GetAllAsync(ct);
        return Ok(mapper.Map<IEnumerable<EmployeeLookupDto>>(users));
    }

    [HttpGet]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery query, CancellationToken ct) =>
        Ok(await employeeService.GetAllAsync(query, ct));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(await employeeService.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ServiceFilter(typeof(ValidationFilter<CreateEmployeeRequest>))]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await employeeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ServiceFilter(typeof(ValidationFilter<UpdateEmployeeRequest>))]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct) =>
        Ok(await employeeService.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await employeeService.DeleteAsync(id, ct);
        return NoContent();
    }
}
