using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using Xunit;

namespace TaskManagement.Application.Tests;

public sealed class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IJwtService> _jwt = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _tokenRepo = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _uow.Setup(u => u.Users).Returns(_userRepo.Object);
        _uow.Setup(u => u.RefreshTokens).Returns(_tokenRepo.Object);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _tokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), default)).Returns(Task.CompletedTask);
        _sut = new AuthService(_uow.Object, _jwt.Object, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsConflictException()
    {
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(new RegisterRequest(
            "John Doe", "john@example.com", "Password1!", "Password1!", UserRole.Employee, null, null));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsAuthResponse()
    {
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");

        var result = await _sut.RegisterAsync(new RegisterRequest(
            "John Doe", "john@example.com", "Password1!", "Password1!", UserRole.Employee, null, null));

        result.AccessToken.Should().Be("access-token");
        // Raw refresh token is returned to client — must not be empty and must not equal the stored hash
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("john@example.com");
        // Verify the token stored in DB is the SHA-256 hash, not the raw token
        _tokenRepo.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(t => t.Token == AuthService.HashToken(result.RefreshToken)),
            default), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_StoresHashedToken_NotRawToken()
    {
        _userRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");

        var result = await _sut.RegisterAsync(new RegisterRequest(
            "Jane", "jane@example.com", "Password1!", "Password1!", UserRole.Employee, null, null));

        var expectedHash = AuthService.HashToken(result.RefreshToken);
        // Raw token and hash must differ
        result.RefreshToken.Should().NotBe(expectedHash);
        _tokenRepo.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(t => t.Token == expectedHash && t.Token != result.RefreshToken),
            default), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsNotFoundException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginRequest("nobody@example.com", "Password1!", false));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsDomainException()
    {
        var user = new User
        {
            FullName = "Jane",
            Email = "jane@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1"),
            Role = UserRole.Employee
        };
        _userRepo.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);

        var act = () => _sut.LoginAsync(new LoginRequest("jane@example.com", "WrongPassword1", false));

        await act.Should().ThrowAsync<DomainException>().WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedUser_ThrowsForbiddenException()
    {
        var user = new User
        {
            FullName = "Jane",
            Email = "jane@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            Role = UserRole.Employee,
            IsActive = false
        };
        _userRepo.Setup(r => r.GetByEmailAsync("jane@example.com", default)).ReturnsAsync(user);

        var act = () => _sut.LoginAsync(new LoginRequest("jane@example.com", "Password1!", false));

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task LoginAsync_WithRememberMe_SetsLongerExpiry()
    {
        var user = new User
        {
            FullName = "Bob",
            Email = "bob@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
            Role = UserRole.Employee
        };
        _userRepo.Setup(r => r.GetByEmailAsync("bob@example.com", default)).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");

        RefreshToken? storedToken = null;
        _tokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), default))
            .Callback<RefreshToken, CancellationToken>((t, _) => storedToken = t)
            .Returns(Task.CompletedTask);

        await _sut.LoginAsync(new LoginRequest("bob@example.com", "Password1!", true));

        storedToken.Should().NotBeNull();
        storedToken!.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsDomainException()
    {
        _tokenRepo.Setup(r => r.GetActiveTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync((RefreshToken?)null);

        var act = () => _sut.RefreshTokenAsync("invalid-raw-token");

        await act.Should().ThrowAsync<DomainException>().WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RevokeTokenAsync_WithInvalidToken_ThrowsDomainException()
    {
        _tokenRepo.Setup(r => r.GetActiveTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync((RefreshToken?)null);

        var act = () => _sut.RevokeTokenAsync("invalid-raw-token");

        await act.Should().ThrowAsync<DomainException>().WithMessage("Token not found or already revoked.");
    }
}
