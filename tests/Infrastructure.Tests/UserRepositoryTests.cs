using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;
using Xunit;

namespace TaskManagement.Infrastructure.Tests;

public sealed class UserRepositoryTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;
    private UserRepository _sut = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _sut = new UserRepository(_context);
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task GetByEmailAsync_WhenUserExists_ReturnsUser()
    {
        var user = CreateUser("alice@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByEmailAsync("alice@test.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("alice@test.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenUserNotFound_ReturnsNull()
    {
        var result = await _sut.GetByEmailAsync("nobody@test.com");
        result.Should().BeNull();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailTaken_ReturnsTrue()
    {
        var user = CreateUser("bob@test.com");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var exists = await _sut.EmailExistsAsync("bob@test.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WhenEmailFree_ReturnsFalse()
    {
        var exists = await _sut.EmailExistsAsync("free@test.com");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleted_Users_AreExcludedFromQueries()
    {
        var user = CreateUser("deleted@test.com");
        user.IsDeleted = true;
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByEmailAsync("deleted@test.com");

        result.Should().BeNull();
    }

    private static User CreateUser(string email) => new()
    {
        FullName = "Test User",
        Email = email,
        PasswordHash = "hash",
        Role = UserRole.Employee
    };
}
