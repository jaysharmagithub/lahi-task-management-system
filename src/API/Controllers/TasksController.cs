using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Task;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Task CRUD. Admins see all tasks; employees see only their own.
/// Completed tasks cannot be edited (enforced in service layer).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class TasksController(ITaskService taskService) : BaseController
{
    /// <summary>Get paginated tasks. Employees automatically filtered to their own tasks.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterQuery query, CancellationToken ct)
    {
        var result = await taskService.GetAllAsync(query, CurrentUserId, IsAdmin, ct);
        return Ok(result);
    }

    /// <summary>Get a single task by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await taskService.GetByIdAsync(id, CurrentUserId, IsAdmin, ct);
        return Ok(result);
    }

    /// <summary>Create a new task and assign it to an employee.</summary>
    [HttpPost]
    [ServiceFilter(typeof(ValidationFilter<CreateTaskRequest>))]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var result = await taskService.CreateAsync(request, CurrentUserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update a task. Completed tasks are immutable.</summary>
    [HttpPut("{id:guid}")]
    [ServiceFilter(typeof(ValidationFilter<UpdateTaskRequest>))]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var result = await taskService.UpdateAsync(id, request, CurrentUserId, IsAdmin, ct);
        return Ok(result);
    }

    /// <summary>Delete a task (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await taskService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Upload a PDF/JPG/PNG attachment (max 5 MB) to a task.</summary>
    [HttpPost("{id:guid}/attachment")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        await using var stream = file.OpenReadStream();
        var result = await taskService.UploadAttachmentAsync(id, stream, file.FileName, file.ContentType, ct);
        return Ok(result);
    }
}
