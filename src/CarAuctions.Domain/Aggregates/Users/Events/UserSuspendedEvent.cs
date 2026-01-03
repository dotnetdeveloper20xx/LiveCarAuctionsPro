using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Users.Events;

public sealed class UserSuspendedEvent : DomainEventBase
{
    public UserId UserId { get; }
    public string Reason { get; }

    public UserSuspendedEvent(UserId userId, string reason)
    {
        UserId = userId;
        Reason = reason;
    }
}
