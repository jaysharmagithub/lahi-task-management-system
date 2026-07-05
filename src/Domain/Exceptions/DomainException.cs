namespace TaskManagement.Domain.Exceptions;

/// <summary>Thrown when a business rule is violated.</summary>
public sealed class DomainException(string message) : Exception(message);
