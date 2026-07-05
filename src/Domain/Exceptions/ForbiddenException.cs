namespace TaskManagement.Domain.Exceptions;

/// <summary>Thrown when an operation is not permitted for the caller.</summary>
public sealed class ForbiddenException(string message) : Exception(message);
