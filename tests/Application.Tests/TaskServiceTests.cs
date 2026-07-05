using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Application.DTOs.Task;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Mapping;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;
using Xunit;

namespace TaskManagement.Application.Tests;

public sealed class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ITaskRepository> _taskRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<INotificationRepository> _notifRepo = new();
    private readonly Mock<IEmailService> _email = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly IMapper _mapper;
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _uow.Setup(u => u.Tasks).Returns(_taskRepo.Object);
        _uow.Setup(u => u.Users).Returns(_userRepo.Object);
        _uow.Setup(u => u.Notifications).Returns(_notifRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _notifRepo.Setup(r => r.AddAsync(It.IsAny<Notification>(), default)).Returns(Task.CompletedTask);
        _email.Setup(e => e.SendTaskAssignedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        _mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();
        _sut = new TaskService(_uow.Object, _mapper, _email.Object, _fileStorage.Object, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskIsCompleted_ThrowsDomainException()
    {
        var taskId = Guid.NewGuid();
        var task = BuildTask(taskId, TaskStatus.Completed);
        _taskRepo.Setup(r => r.GetWithDetailsAsync(taskId, default)).ReturnsAsync(task);

        var act = () => _sut.UpdateAsync(taskId,
            new UpdateTaskRequest("Title", null, TaskPriority.Low, TaskStatus.InProgress, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), task.AssignedToId),
            task.AssignedToId, isAdmin: false);

        await act.Should().ThrowAsync<DomainException>().WithMessage("Completed tasks cannot be edited.");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeAccessesOtherTask_ThrowsForbiddenException()
    {
        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid(); // different user
        var task = BuildTask(taskId, TaskStatus.Pending, ownerId);
        _taskRepo.Setup(r => r.GetWithDetailsAsync(taskId, default)).ReturnsAsync(task);

        var act = () => _sut.GetByIdAsync(taskId, requesterId, isAdmin: false);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenAdminAccessesAnyTask_Succeeds()
    {
        var taskId = Guid.NewGuid();
        var task = BuildTask(taskId, TaskStatus.Pending);
        _taskRepo.Setup(r => r.GetWithDetailsAsync(taskId, default)).ReturnsAsync(task);

        var result = await _sut.GetByIdAsync(taskId, Guid.NewGuid(), isAdmin: true);

        result.Should().NotBeNull();
        result.Id.Should().Be(taskId);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        _taskRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((TaskItem?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UploadAttachmentAsync_WithInvalidContentType_ThrowsDomainException()
    {
        var taskId = Guid.NewGuid();
        var task = BuildTask(taskId, TaskStatus.Pending);
        _taskRepo.Setup(r => r.GetWithDetailsAsync(taskId, default)).ReturnsAsync(task);

        using var stream = new MemoryStream(new byte[100]);
        var act = () => _sut.UploadAttachmentAsync(taskId, stream, "file.exe", "application/octet-stream");

        await act.Should().ThrowAsync<DomainException>().WithMessage("*PDF, JPG, and PNG*");
    }

    private static TaskItem BuildTask(Guid id, TaskStatus status, Guid? assignedToId = null)
    {
        var userId = assignedToId ?? Guid.NewGuid();
        return new TaskItem
        {
            Id = id,
            Title = "Test Task",
            Priority = TaskPriority.Medium,
            Status = status,
            StartDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(3),
            AssignedToId = userId,
            CreatedById = Guid.NewGuid(),
            AssignedTo = new User { FullName = "Employee", Email = "emp@test.com", PasswordHash = "x", Role = UserRole.Employee },
            CreatedBy = new User { FullName = "Admin", Email = "admin@test.com", PasswordHash = "x", Role = UserRole.Admin }
        };
    }
}
