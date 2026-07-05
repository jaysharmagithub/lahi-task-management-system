namespace TaskManagement.Application.DTOs.Common;

/// <summary>Query parameters shared by all paginated list endpoints.</summary>
public record PaginationQuery(
    int Page = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = "asc",
    string? Search = null);
