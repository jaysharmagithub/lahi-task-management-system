using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.API.Filters;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Handles user registration, login, token refresh, and logout.
/// Authentication flow:
///   1. POST /register or /login → returns access token (15 min) + refresh token
///   2. Client stores refresh token (httpOnly cookie recommended in production)
///   3. POST /refresh → rotates refresh token, issues new access token
///   4. POST /logout → revokes refresh token
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController(IAuthService authService) : BaseController
{
    /// <summary>Register a new user (Admin or Employee).</summary>
    [HttpPost("register")]
    [ServiceFilter(typeof(ValidationFilter<RegisterRequest>))]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>Login with email and password. Set RememberMe=true for a 30-day refresh token.</summary>
    [HttpPost("login")]
    [ServiceFilter(typeof(ValidationFilter<LoginRequest>))]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Exchange a valid refresh token for a new access token (token rotation).</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await authService.RefreshTokenAsync(request.RefreshToken, ct);
        return Ok(result);
    }

    /// <summary>Revoke the current refresh token (logout).</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        await authService.RevokeTokenAsync(request.RefreshToken, ct);
        return NoContent();
    }
}
