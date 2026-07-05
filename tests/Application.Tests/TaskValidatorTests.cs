using FluentAssertions;
using TaskManagement.Application.Validators;
using TaskManagement.Application.DTOs.Task;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;
using Xunit;

namespace TaskManagement.Application.Tests;

public sealed class TaskValidatorTests
{
    private readonly CreateTaskRequestValidator _validator = new();

    [Fact]
    public async Task CreateTask_WhenDueDateBeforeStartDate_Fails()
    {
        var request = new CreateTaskRequest(
            "Title", null, TaskPriority.Low, TaskStatus.Pending,
            StartDate: DateTime.UtcNow.AddDays(5),
            DueDate: DateTime.UtcNow.AddDays(1),
            AssignedToId: Guid.NewGuid());

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_Fails()
    {
        var request = new CreateTaskRequest(
            "", null, TaskPriority.Low, TaskStatus.Pending,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Guid.NewGuid());

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task CreateTask_WithValidData_Passes()
    {
        var request = new CreateTaskRequest(
            "Valid Task", "Description", TaskPriority.High, TaskStatus.Pending,
            DateTime.UtcNow, DateTime.UtcNow.AddDays(7), Guid.NewGuid());

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }
}
