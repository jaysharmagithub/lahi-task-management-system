using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Task;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetAllAsync(TaskFilterQuery query, Guid requesterId, bool isAdmin, CancellationToken ct = default);
    Task<TaskDto> GetByIdAsync(Guid id, Guid requesterId, bool isAdmin, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid createdById, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid requesterId, bool isAdmin, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<TaskDto> UploadAttachmentAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
}
