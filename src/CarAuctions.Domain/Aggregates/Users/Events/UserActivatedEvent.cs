using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Users.Events;

public sealed class UserActivatedEvent : DomainEventBase
{
    public UserId UserId { get; }

    public UserActivatedEvent(UserId userId)
    {
        UserId = userId;
    }
}
