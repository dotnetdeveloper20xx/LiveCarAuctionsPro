namespace CarAuctions.Domain.Common;

/// <summary>
/// Base class for aggregate roots. An aggregate root is the entry point to an aggregate,
/// ensuring consistency and invariants are maintained.
/// </summary>
/// <typeparam name="TId">The type of the aggregate root identifier.</typeparam>
public abstract class AggregateRoot<TId> : BaseEntity<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets or sets the version number for optimistic concurrency control.
    /// </summary>
    public int Version { get; protected set; }

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// For EF Core.
    /// </summary>
    protected AggregateRoot() : base()
    {
    }
}
