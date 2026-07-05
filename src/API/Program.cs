using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TaskManagement.API.Extensions;
using TaskManagement.API.Filters;
using TaskManagement.API.Middleware;
using TaskManagement.Application;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Employee;
using TaskManagement.Application.DTOs.Task;
using TaskManagement.Infrastructure;
using TaskManagement.Infrastructure.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "CHANGE_ME_TO_A_32_CHAR_MIN_SECRET_KEY_IN_PRODUCTION";
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // Enforce camelCase JSON serialization
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

    // Register Validation Filters
    builder.Services.AddScoped<ValidationFilter<RegisterRequest>>();
    builder.Services.AddScoped<ValidationFilter<LoginRequest>>();
    builder.Services.AddScoped<ValidationFilter<CreateEmployeeRequest>>();
    builder.Services.AddScoped<ValidationFilter<UpdateEmployeeRequest>>();
    builder.Services.AddScoped<ValidationFilter<CreateTaskRequest>>();
    builder.Services.AddScoped<ValidationFilter<UpdateTaskRequest>>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    await app.MigrateDbAsync();

    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
        c.RoutePrefix = string.Empty;
    });

    app.UseSecurityHeaders();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
