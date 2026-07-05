using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Common;
using TaskManagement.Application.DTOs.Task;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Services;

public sealed class TaskService(
    IUnitOfWork uow,
    IMapper mapper,
    IEmailService emailService,
    IFileStorageService fileStorage,
    ILogger<TaskService> logger) : ITaskService
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "application/pdf"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public async Task<PagedResult<TaskDto>> GetAllAsync(TaskFilterQuery query, Guid requesterId, bool isAdmin, CancellationToken ct = default)
    {
        var userId = isAdmin ? (Guid?)null : requesterId;
        var (items, total) = await uow.Tasks.GetPagedAsync(query, userId, ct);
        return new PagedResult<TaskDto>(mapper.Map<IReadOnlyList<TaskDto>>(items), total, query.Page, query.PageSize);
    }

    public async Task<TaskDto> GetByIdAsync(Guid id, Guid requesterId, bool isAdmin, CancellationToken ct = default)
    {
        var task = await uow.Tasks.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        if (!isAdmin && task.AssignedToId != requesterId)
            throw new ForbiddenException("Access denied.");

        return mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, Guid createdById, CancellationToken ct = default)
    {
        var assignee = await uow.Users.GetByIdAsync(request.AssignedToId, ct)
            ?? throw new NotFoundException(nameof(User), request.AssignedToId);

        var creator = await uow.Users.GetByIdAsync(createdById, ct)
            ?? throw new NotFoundException(nameof(User), createdById);

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = request.Status,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId,
            CreatedById = createdById
        };

        await uow.Tasks.AddAsync(task, ct);

        var notification = new Notification
        {
            Message = $"You have been assigned task: {task.Title}",
            Type = NotificationType.TaskAssigned,
            UserId = request.AssignedToId,
            TaskId = task.Id
        };
        await uow.Notifications.AddAsync(notification, ct);
        await uow.SaveChangesAsync(ct);

        await emailService.SendTaskAssignedAsync(assignee.Email, assignee.FullName, task.Title, ct);

        // Manually set navigation properties for the mapper to prevent crash
        task.AssignedTo = assignee;
        task.CreatedBy = creator;

        return mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid requesterId, bool isAdmin, CancellationToken ct = default)
    {
        var task = await uow.Tasks.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        if (!isAdmin && task.Status == TaskStatus.Completed)
            throw new DomainException("Completed tasks cannot be edited.");

        if (!isAdmin && task.AssignedToId != requesterId)
            throw new ForbiddenException("Access denied.");

        var wasReassigned = task.AssignedToId != request.AssignedToId;
        var isNowCompleted = request.Status == TaskStatus.Completed && task.Status != TaskStatus.Completed;

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.StartDate = request.StartDate;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        uow.Tasks.Update(task);

        if (wasReassigned)
        {
            var newAssignee = await uow.Users.GetByIdAsync(request.AssignedToId, ct)
                ?? throw new NotFoundException(nameof(User), request.AssignedToId);

            task.AssignedTo = newAssignee;

            await uow.Notifications.AddAsync(new Notification
            {
                Message = $"You have been assigned task: {task.Title}",
                Type = NotificationType.TaskAssigned,
                UserId = request.AssignedToId,
                TaskId = task.Id
            }, ct);
            await emailService.SendTaskAssignedAsync(newAssignee.Email, newAssignee.FullName, task.Title, ct);
        }

        if (isNowCompleted)
        {
            await uow.Notifications.AddAsync(new Notification
            {
                Message = $"Task '{task.Title}' has been marked complete.",
                Type = NotificationType.TaskCompleted,
                UserId = task.AssignedToId,
                TaskId = task.Id
            }, ct);
            await emailService.SendTaskCompletedAsync(task.AssignedTo.Email, task.AssignedTo.FullName, task.Title, ct);
        }

        await uow.SaveChangesAsync(ct);
        return mapper.Map<TaskDto>(task);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var task = await uow.Tasks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        uow.Tasks.Update(task);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<TaskDto> UploadAttachmentAsync(Guid id, Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        if (!AllowedContentTypes.Contains(contentType))
            throw new DomainException("Only PDF, JPG, and PNG files are allowed.");

        if (fileStream.Length > MaxFileSizeBytes)
            throw new DomainException("File size must not exceed 5 MB.");

        var task = await uow.Tasks.GetWithDetailsAsync(id, ct)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        if (!string.IsNullOrEmpty(task.AttachmentPath))
            await fileStorage.DeleteAsync(task.AttachmentPath, ct);

        var path = await fileStorage.SaveAsync(fileStream, fileName, contentType, ct);
        task.AttachmentPath = path;
        task.AttachmentFileName = fileName;
        task.UpdatedAt = DateTime.UtcNow;

        uow.Tasks.Update(task);
        await uow.SaveChangesAsync(ct);
        return mapper.Map<TaskDto>(task);
    }
}
