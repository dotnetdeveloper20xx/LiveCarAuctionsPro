using MediatR;

namespace CarAuctions.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}
