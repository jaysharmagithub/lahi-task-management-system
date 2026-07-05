namespace TaskManagement.Domain.Events;

/// <summary>Marker interface for domain events.</summary>
public interface IDomainEvent;

public record TaskAssignedEvent(Guid TaskId, Guid AssignedToId, string TaskTitle) : IDomainEvent;
public record TaskCompletedEvent(Guid TaskId, Guid AssignedToId, string TaskTitle) : IDomainEvent;
public record TaskDueSoonEvent(Guid TaskId, Guid AssignedToId, string TaskTitle, DateTime DueDate) : IDomainEvent;
