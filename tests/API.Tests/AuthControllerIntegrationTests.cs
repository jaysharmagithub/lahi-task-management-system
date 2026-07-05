using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using Xunit;

namespace TaskManagement.API.Tests;

public sealed class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server with in-memory for tests
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("IntegrationTestDb"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var request = new RegisterRequest(
            "Integration User", "integration@test.com", "Password1!", "Password1!",
            UserRole.Employee, "Engineering", "Developer");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.User.Email.Should().Be("integration@test.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var request = new RegisterRequest(
            "Dup User", "dup@test.com", "Password1!", "Password1!", UserRole.Employee, null, null);

        await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns400()
    {
        var request = new LoginRequest("nobody@test.com", "WrongPass1!", false);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTasks_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
