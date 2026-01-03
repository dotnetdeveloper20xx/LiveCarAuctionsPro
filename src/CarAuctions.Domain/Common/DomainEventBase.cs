namespace CarAuctions.Domain.Common;

/// <summary>
/// Base class for domain events providing common functionality.
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    protected DomainEventBase()
    {
        OccurredOn = DateTime.UtcNow;
    }

    public DateTime OccurredOn { get; }
}
