using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Bids.Events;

public sealed class ProxyBidPlacedEvent : DomainEventBase
{
    public BidId BidId { get; }
    public AuctionId AuctionId { get; }
    public UserId BidderId { get; }
    public Money CurrentAmount { get; }
    public Money MaxAmount { get; }

    public ProxyBidPlacedEvent(BidId bidId, AuctionId auctionId, UserId bidderId, Money currentAmount, Money maxAmount)
    {
        BidId = bidId;
        AuctionId = auctionId;
        BidderId = bidderId;
        CurrentAmount = currentAmount;
        MaxAmount = maxAmount;
    }
}
