using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Users.Events;

public sealed class UserKycVerifiedEvent : DomainEventBase
{
    public UserId UserId { get; }

    public UserKycVerifiedEvent(UserId userId)
    {
        UserId = userId;
    }
}
