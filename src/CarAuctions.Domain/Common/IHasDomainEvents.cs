namespace CarAuctions.Domain.Common;

/// <summary>
/// Interface for entities that can raise domain events.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this entity.
    /// </summary>
    void ClearDomainEvents();
}
