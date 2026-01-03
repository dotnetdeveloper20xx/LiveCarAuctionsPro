using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions.Events;

public sealed class AuctionStartedEvent : DomainEventBase
{
    public AuctionId AuctionId { get; }

    public AuctionStartedEvent(AuctionId auctionId)
    {
        AuctionId = auctionId;
    }
}
