using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Payments.Events;

public record PaymentFailedEvent(
    PaymentId PaymentId,
    AuctionId AuctionId,
    UserId UserId,
    string Reason
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
