using FluentAssertions;
using TaskManagement.Application.Validators;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Enums;
using Xunit;

namespace TaskManagement.Application.Tests;

public sealed class AuthValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Theory]
    [InlineData("password")]       // no uppercase, no number
    [InlineData("PASSWORD1")]      // no lowercase
    [InlineData("Password")]       // no number
    [InlineData("Pass1")]          // too short
    public async Task RegisterValidator_WithWeakPassword_Fails(string password)
    {
        var request = new RegisterRequest("John", "john@test.com", password, password, UserRole.Employee, null, null);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterValidator_WithMismatchedPasswords_Fails()
    {
        var request = new RegisterRequest("John", "john@test.com", "Password1!", "Different1!", UserRole.Employee, null, null);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Fact]
    public async Task RegisterValidator_WithInvalidEmail_Fails()
    {
        var request = new RegisterRequest("John", "not-an-email", "Password1!", "Password1!", UserRole.Employee, null, null);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterValidator_WithValidData_Passes()
    {
        var request = new RegisterRequest("John Doe", "john@test.com", "Password1!", "Password1!", UserRole.Employee, null, null);
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }
}
