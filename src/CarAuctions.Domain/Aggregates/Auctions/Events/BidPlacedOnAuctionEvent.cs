using CarAuctions.Domain.Aggregates.Bids;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Auctions.Events;

public sealed class BidPlacedOnAuctionEvent : DomainEventBase
{
    public AuctionId AuctionId { get; }
    public BidId BidId { get; }
    public UserId BidderId { get; }
    public Money Amount { get; }

    public BidPlacedOnAuctionEvent(AuctionId auctionId, BidId bidId, UserId bidderId, Money amount)
    {
        AuctionId = auctionId;
        BidId = bidId;
        BidderId = bidderId;
        Amount = amount;
    }
}
