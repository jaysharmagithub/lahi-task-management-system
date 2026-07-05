using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Auth;

public record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword,
    UserRole Role,
    string? Department,
    string? Designation);

public record LoginRequest(
    string Email,
    string Password,
    bool RememberMe);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    UserDto User);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    string? Department,
    string? Designation);
