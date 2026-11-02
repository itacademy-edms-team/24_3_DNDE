namespace Common.Application;

/// <summary>
/// Marker for domain events dispatched in-process (MediatR notifications).
/// </summary>
public interface IDomainEvent : MediatR.INotification;
