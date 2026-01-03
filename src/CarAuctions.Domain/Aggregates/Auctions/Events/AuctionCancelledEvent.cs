using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions.Events;

public sealed class AuctionCancelledEvent : DomainEventBase
{
    public AuctionId AuctionId { get; }
    public string Reason { get; }

    public AuctionCancelledEvent(AuctionId auctionId, string reason)
    {
        AuctionId = auctionId;
        Reason = reason;
    }
}
