namespace TaskManagement.Domain.Exceptions;

/// <summary>Thrown when a requested resource does not exist.</summary>
public sealed class NotFoundException(string name, object key)
    : Exception($"{name} with key '{key}' was not found.");
