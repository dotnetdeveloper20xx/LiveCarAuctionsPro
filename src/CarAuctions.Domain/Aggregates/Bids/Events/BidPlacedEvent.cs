using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Bids.Events;

public sealed class BidPlacedEvent : DomainEventBase
{
    public BidId BidId { get; }
    public AuctionId AuctionId { get; }
    public UserId BidderId { get; }
    public Money Amount { get; }

    public BidPlacedEvent(BidId bidId, AuctionId auctionId, UserId bidderId, Money amount)
    {
        BidId = bidId;
        AuctionId = auctionId;
        BidderId = bidderId;
        Amount = amount;
    }
}
