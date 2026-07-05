namespace TaskManagement.Application.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportToExcelAsync<T>(IReadOnlyList<T> rows, string sheetName, CancellationToken ct = default);
    Task<byte[]> ExportToCsvAsync<T>(IReadOnlyList<T> rows, CancellationToken ct = default);
}
