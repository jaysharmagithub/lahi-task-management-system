using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Domain.Enums;

namespace TaskManagement.API.Controllers;

/// <summary>Base controller providing shared helpers for all API controllers.</summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User identity not found."));

    protected bool IsAdmin =>
        User.IsInRole(UserRole.Admin.ToString());
}
