using CarAuctions.Domain.Aggregates.Auctions;
using CarAuctions.Domain.Aggregates.Users;
using CarAuctions.Domain.Common;

namespace CarAuctions.Domain.Aggregates.Bids.Events;

public sealed class BidWithdrawnEvent : DomainEventBase
{
    public BidId BidId { get; }
    public AuctionId AuctionId { get; }
    public UserId BidderId { get; }
    public string Reason { get; }

    public BidWithdrawnEvent(BidId bidId, AuctionId auctionId, UserId bidderId, string reason)
    {
        BidId = bidId;
        AuctionId = auctionId;
        BidderId = bidderId;
        Reason = reason;
    }
}
