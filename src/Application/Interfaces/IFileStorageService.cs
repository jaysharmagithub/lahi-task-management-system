namespace TaskManagement.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string filePath, CancellationToken ct = default);
}
