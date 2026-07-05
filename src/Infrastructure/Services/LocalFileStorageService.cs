using Microsoft.Extensions.Configuration;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Services;

/// <summary>Stores uploaded files on the local filesystem with path traversal protection.</summary>
public sealed class LocalFileStorageService(IConfiguration configuration) : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private readonly string _basePath = Path.GetFullPath(configuration["FileStorage:BasePath"] ?? "uploads");

    public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        // Sanitize: strip directory components, validate extension
        var safeFileName = Path.GetFileName(fileName);
        var extension = Path.GetExtension(safeFileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File extension '{extension}' is not permitted.");

        Directory.CreateDirectory(_basePath);
        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_basePath, uniqueName);

        // Verify the resolved path is still inside _basePath (defence-in-depth)
        if (!Path.GetFullPath(fullPath).StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid file path.");

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream, ct);

        // Return relative path — never expose absolute server paths to callers
        return uniqueName;
    }

    public Task DeleteAsync(string filePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, Path.GetFileName(filePath));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
