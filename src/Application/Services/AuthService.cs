using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Application.Services;

/// <summary>Handles registration, login, token refresh, and revocation.</summary>
public sealed class AuthService(
    IUnitOfWork uow,
    IJwtService jwtService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await uow.Users.EmailExistsAsync(request.Email, ct))
            throw new ConflictException($"Email '{request.Email}' is already registered.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            Role = request.Role,
            Department = request.Department,
            Designation = request.Designation
        };

        await uow.Users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("User {Email} registered with role {Role}", user.Email, user.Role);
        return await BuildAuthResponseAsync(user, rememberMe: false, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await uow.Users.GetByEmailAsync(request.Email.ToLowerInvariant(), ct)
            ?? throw new NotFoundException(nameof(User), request.Email);

        if (!user.IsActive)
            throw new ForbiddenException("Account is deactivated.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Invalid credentials.");

        logger.LogInformation("User {Email} logged in", user.Email);
        return await BuildAuthResponseAsync(user, request.RememberMe, ct);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // Hash the incoming token before DB lookup (tokens stored as SHA-256 hashes)
        var tokenHash = HashToken(refreshToken);
        var token = await uow.RefreshTokens.GetActiveTokenAsync(tokenHash, ct)
            ?? throw new DomainException("Invalid or expired refresh token.");

        var user = await uow.Users.GetByIdAsync(token.UserId, ct)
            ?? throw new NotFoundException(nameof(User), token.UserId);

        // Rotate: revoke old, issue new
        var rawNewToken = GenerateRawToken();
        var newTokenHash = HashToken(rawNewToken);

        token.IsRevoked = true;
        token.RevokedReason = "Replaced";
        token.ReplacedByToken = newTokenHash;

        var newRefresh = new RefreshToken
        {
            Token = newTokenHash,
            UserId = user.Id,
            ExpiresAt = token.ExpiresAt > DateTime.UtcNow.AddDays(1)
                ? DateTime.UtcNow.AddDays(30)
                : DateTime.UtcNow.AddDays(7)
        };

        await uow.RefreshTokens.AddAsync(newRefresh, ct);
        await uow.SaveChangesAsync(ct);

        var accessToken = jwtService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, rawNewToken, DateTime.UtcNow.AddMinutes(15), MapUserDto(user));
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await uow.RefreshTokens.GetActiveTokenAsync(tokenHash, ct)
            ?? throw new DomainException("Token not found or already revoked.");

        token.IsRevoked = true;
        token.RevokedReason = "Logout";
        await uow.SaveChangesAsync(ct);
    }

    /// <summary>Fully async — no blocking calls. Issues tokens and persists refresh token.</summary>
    private async Task<AuthResponse> BuildAuthResponseAsync(User user, bool rememberMe, CancellationToken ct)
    {
        var accessToken = jwtService.GenerateAccessToken(user);
        var rawToken = GenerateRawToken();
        var tokenHash = HashToken(rawToken);

        var days = rememberMe ? 30 : 7;
        var refresh = new RefreshToken
        {
            Token = tokenHash,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        };

        await uow.RefreshTokens.AddAsync(refresh, ct);
        await uow.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, rawToken, DateTime.UtcNow.AddMinutes(15), MapUserDto(user));
    }

    /// <summary>Generates a cryptographically random opaque token string.</summary>
    private static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <summary>SHA-256 hashes a raw token before storage — raw token is never persisted.</summary>
    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static UserDto MapUserDto(User u) =>
        new(u.Id, u.FullName, u.Email, u.Role, u.Department, u.Designation);
}
