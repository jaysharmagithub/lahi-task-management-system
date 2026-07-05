using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;
using System.Reflection;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Infrastructure.Services;

/// <summary>
/// Implements Excel and CSV export using ClosedXML and CsvHelper.
/// Kept in Infrastructure so Application layer has zero dependency on export libraries.
/// </summary>
public sealed class ExportService : IExportService
{
    public Task<byte[]> ExportToExcelAsync<T>(IReadOnlyList<T> rows, string sheetName, CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Header row
        for (var col = 0; col < properties.Length; col++)
            ws.Cell(1, col + 1).Value = properties[col].Name;

        // Data rows
        for (var row = 0; row < rows.Count; row++)
        {
            for (var col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(rows[row]);
                ws.Cell(row + 2, col + 1).Value = value switch
                {
                    DateTime dt => dt.ToString("yyyy-MM-dd"),
                    _ => value?.ToString() ?? string.Empty
                };
            }
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public async Task<byte[]> ExportToCsvAsync<T>(IReadOnlyList<T> rows, CancellationToken ct = default)
    {
        using var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream, leaveOpen: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(rows, ct);
        await writer.FlushAsync(ct);
        return stream.ToArray();
    }
}
