using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Notification;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>In-app notifications for the authenticated user.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class NotificationsController(INotificationService notificationService) : BaseController
{
    /// <summary>Get all notifications for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await notificationService.GetUserNotificationsAsync(CurrentUserId, ct);
        return Ok(result);
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await notificationService.MarkAsReadAsync(id, CurrentUserId, ct);
        return NoContent();
    }

    /// <summary>Mark all notifications as read for the current user.</summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await notificationService.MarkAllAsReadAsync(CurrentUserId, ct);
        return NoContent();
    }
}
