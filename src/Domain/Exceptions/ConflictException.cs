namespace TaskManagement.Domain.Exceptions;

/// <summary>Thrown when a uniqueness constraint is violated (e.g. duplicate email).</summary>
public sealed class ConflictException(string message) : Exception(message);
