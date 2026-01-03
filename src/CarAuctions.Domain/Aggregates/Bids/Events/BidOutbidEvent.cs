using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Bids.Events;

public sealed class BidOutbidEvent : DomainEventBase
{
    public BidId BidId { get; }
    public AuctionId AuctionId { get; }
    public UserId BidderId { get; }

    public BidOutbidEvent(BidId bidId, AuctionId auctionId, UserId bidderId)
    {
        BidId = bidId;
        AuctionId = auctionId;
        BidderId = bidderId;
    }
}
