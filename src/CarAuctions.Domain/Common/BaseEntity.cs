namespace CarAuctions.Domain.Common;

/// <summary>
/// Base class for all domain entities with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected BaseEntity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// For EF Core.
    /// </summary>
    protected BaseEntity()
    {
        Id = default!;
    }

    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public TId Id { get; private set; }

    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Raises a domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from this entity.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public bool Equals(BaseEntity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseEntity<TId>);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return !Equals(left, right);
    }
}
